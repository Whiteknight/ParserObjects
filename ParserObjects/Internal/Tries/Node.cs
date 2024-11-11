using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal.Tries;

public class Node<TKey, TResult> : Dictionary<ValueTuple<TKey>, (Node<TKey, TResult>? Node, bool HasValue, TResult? Value)>
{
    /* I generally don't like concrete inheritance, but in this case it's an optimization to
     * make the RootNode and Node be IS-A Dictionary. If we made it a struct there would be the same
     * number of allocations except we wouldn't be able to have reference behavior for setting
     * HasResult, Result and MaxDepth.
     */

    public Node()
    {
        Result = default;
    }

    public Node(IEqualityComparer<ValueTuple<TKey>> comparer)
        : base(comparer)
    {
        Result = default;
    }

    public TResult? Result { get; private set; }

    public Node<TKey, TResult> GetOrAddChild(TKey key)
    {
        Assert.ArgumentNotNull(key);
        var wrappedKey = new ValueTuple<TKey>(key);
        if (ContainsKey(wrappedKey))
        {
            if (this[wrappedKey].Node != null)
                return this[wrappedKey].Node!;
            var node = new Node<TKey, TResult>(Comparer);
            this[wrappedKey] = (node, this[wrappedKey].HasValue, this[wrappedKey].Value);
            return node;
        }

        var newNode = new Node<TKey, TResult>(Comparer);
        Add(wrappedKey, (newNode, false, default));
        return newNode;
    }
}

// RootNode is the Trie implementation. InsertableTrie and ReadableTrie are just wrappers around
// RootNode to provide limited access to a subset of functionality depending on whether we are in
// build-up or parse phases.
public class RootNode<TKey, TResult> : Node<TKey, TResult>
{
    public RootNode()
    {
        Editable = true;
        MaxDepth = 0;
    }

    public RootNode(IEqualityComparer<TKey> comparer)
        : base(new WrappedEqualityComparer(comparer))
    {
        Editable = true;
        MaxDepth = 0;
    }

    // A flag that says whether we are in write or read mode. In read mode we should not be
    // trying add more data.
    public bool Editable { get; private set; }

    // Maximum depth of the longest pattern in the Trie. This value is only set on the root node
    public int MaxDepth { get; private set; }

    // Sets a max depth of the trie, recalculated each time a new entry is added. That way we can
    // do things like rent arrays for temporary traversal and know how much space we need.
    private void SetPatternDepth(int depth)
    {
        if (depth > MaxDepth)
            MaxDepth = depth;
    }

    public void Lock()
    {
        Editable = false;
    }

    public PartialResult<TResult> Get(ISequence<TKey> keys)
    {
        /* Trie Get is greedy. We search to the maximum depth where items from keys continue to
         * match nodes. When we run out of matches, the current node we are pointing at might be
         * an interior node and not have a value. In that case we have to recurse back up the stack
         * of previously visited nodes until we do find one with a value.
         *
         * As we traverse nodes we also keep the SequenceCheckpoint of the input sequence at that
         * point. When we recurse to find a node with a value, we can jump back to that checkpoint
         * and continue the parse from that point.
         */
        Node<TKey, TResult> current = this;

        // The node and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = ArrayPool<BacktrackNode>.Shared.Rent(MaxDepth);
        var index = 0;
        var startCont = keys.Checkpoint();
        var startConsumed = keys.Consumed;

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging and can start looking for a value
            // to return.
            if (keys.IsAtEnd || current.Count == 0)
                return FindBestResult(index, previous, keys, startCont, startConsumed);

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child, find the best value
            if (!current.TryGetValue(wrappedKey, out var currentValue))
                return FindBestResult(index, previous, keys, startCont, startConsumed);

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration. We only need
            // to take a Checkpoint if there's a value on this node. .Checkpoint() is cheap but
            // never free.

            var (node, hasValue, value) = currentValue;
            if (hasValue)
            {
                var cont = keys.Checkpoint();
                previous[index++] = new BacktrackNode(true, value!, cont);
            }

            if (node == null)
                return FindBestResult(index, previous, keys, startCont, startConsumed);

            current = node!;
        }
    }

    private readonly record struct BacktrackNode(bool HasValue, TResult Value, SequenceCheckpoint Checkpoint);

    private static PartialResult<TResult> FindBestResult(int index, BacktrackNode[] previous, ISequence<TKey> keys, SequenceCheckpoint startCont, int startConsumed)
    {
        while (index > 0)
        {
            var (hasValue, value, cont) = previous[--index];
            if (hasValue)
            {
                cont.Rewind();
                ArrayPool<BacktrackNode>.Shared.Return(previous);
                return new PartialResult<TResult>(value, keys.Consumed - startConsumed);
            }
        }

        // No node matched, so return failure
        startCont.Rewind();
        ArrayPool<BacktrackNode>.Shared.Return(previous);
        return new PartialResult<TResult>("Trie does not contain matching item");
    }

    public bool CanGet(ISequence<TKey> keys)
    {
        Node<TKey, TResult> current = this;

        // The node, and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = ArrayPool<BacktrackNode>.Shared.Rent(MaxDepth);
        var index = 0;
        var startCont = keys.Checkpoint();

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging and can start looking for a value
            // to return.
            if (keys.IsAtEnd || current.Count == 0)
                return FindBestResult(index, previous, keys, startCont, 0).Success;

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var wrappedKey = new ValueTuple<TKey>(key);
            if (!current.TryGetValue(wrappedKey, out var currentValue))
                return FindBestResult(index, previous, keys, startCont, 0).Success;

            var (node, hasValue, value) = currentValue;
            if (hasValue)
            {
                var cont = keys.Checkpoint();
                previous[index++] = new BacktrackNode(hasValue, value!, cont);
            }

            if (node == null)
                return FindBestResult(index, previous, keys, startCont, 0).Success;

            current = node!;
        }
    }

    public IReadOnlyList<ResultAlternative<TResult>> GetMany(ISequence<TKey> keys)
    {
        Node<TKey, TResult> current = this;
        var results = new List<ResultAlternative<TResult>>();

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging so return any values we have
            if (keys.IsAtEnd || current.Count == 0)
                return results;

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child in this node, return the results we have
            if (!current.TryGetValue(wrappedKey, out var currentValue))
                return results;

            var (node, hasValue, value) = currentValue;

            if (hasValue)
            {
                var cont = keys.Checkpoint();
                results.Add(ResultAlternative<TResult>.Ok(value!, cont.Consumed, cont));
            }

            if (node == null)
                return results;

            current = node!;
        }
    }

    public bool TryAdd(IReadOnlyList<TKey> keyList, TResult value)
    {
        if (!Editable)
            throw new TrieInsertException("Cannot insert new items into a Trie which has been locked");

        Node<TKey, TResult> current = this;
        for (int i = 0; i < keyList.Count - 1; i++)
            current = current.GetOrAddChild(keyList[i]);

        var finalKey = keyList[^1];
        var wrappedKey = new ValueTuple<TKey>(finalKey);
        if (current.TryGetValue(wrappedKey, out var currentValue))
        {
            if (currentValue.HasValue)
            {
                if (current[wrappedKey].Value!.Equals(value))
                    return false;
                throw new TrieInsertException("The result value has already been set for this input sequence");
            }

            current[wrappedKey] = (current[wrappedKey].Node, true, value);
            SetPatternDepth(keyList.Count);
            return true;
        }

        current.Add(wrappedKey, (null, true, value));
        SetPatternDepth(keyList.Count);
        return true;
    }

    // Node and RootNode IS-A Dictionary, and Dictionary has annotations to prevent nullable types
    // from being used as Keys. However, due to the generic nature of parsing, we may be parsing
    // a sequence of nullable types such as Tokens. For that reason, we wrap our key values in
    // a non-nullable struct ValueTuple<TKey> which should be free to allocate and cannot be null
    // (although the only internal value is a null pointer, so a by-value comparsion would be
    // against a null pointer which is treated like untyped memory). We use this equality comparer
    // to wrap up an IEqualityComparer<TKey> so we can pass it to the dictionary.
    private sealed class WrappedEqualityComparer : IEqualityComparer<ValueTuple<TKey>>
    {
        public IEqualityComparer<TKey> _inner;

        public WrappedEqualityComparer(IEqualityComparer<TKey> inner)
        {
            _inner = inner;
        }

        public bool Equals(ValueTuple<TKey> x, ValueTuple<TKey> y)
            => _inner.Equals(x.Item1, y.Item1);

        public int GetHashCode([DisallowNull] ValueTuple<TKey> obj)
            => obj.Item1 is null ? 0 : _inner.GetHashCode(obj.Item1!);
    }
}
