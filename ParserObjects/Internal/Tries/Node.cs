using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal.Tries;

public class Node<TKey, TResult> : Dictionary<ValueTuple<TKey>, Node<TKey, TResult>>
{
    /* I generally don't like concrete inheritance, but in this case it's an optimization to
     * make the RootNode and Node be IS-A Dictionary. If we made it a struct there would be the same
     * number of allocations except we wouldn't be able to have reference behavior for setting
     * HasResult, Result and MaxDepth.
     */

    public Node()
    {
        HasResult = false;
        Result = default;
    }

    public Node(IEqualityComparer<ValueTuple<TKey>> comparer)
        : base(comparer)
    {
        HasResult = false;
        Result = default;
    }

    public bool HasResult { get; private set; }

    public TResult? Result { get; private set; }

    public Node<TKey, TResult> GetOrAddChild(TKey key)
    {
        Assert.ArgumentNotNull(key, nameof(key));
        var wrappedKey = new ValueTuple<TKey>(key);
        if (ContainsKey(wrappedKey))
            return this[wrappedKey];
        var newNode = new Node<TKey, TResult>(Comparer);
        Add(wrappedKey, newNode);
        return newNode;
    }

    public bool TryAddResult(TResult result)
    {
        if (!HasResult || Result == null)
        {
            HasResult = true;
            Result = result;
            return true;
        }

        if (Result.Equals(result))
            return false;

        throw new TrieInsertException("The result value has already been set for this input sequence");
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
        previous[index++] = new BacktrackNode(current, startCont);

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
            if (!current.ContainsKey(wrappedKey))
                return FindBestResult(index, previous, keys, startCont, startConsumed);

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration. We only need
            // to take a Checkpoint if there's a value on this node. .Checkpoint() is cheap but
            // never free.
            current = current[wrappedKey];
            var cont = current.HasResult ? keys.Checkpoint() : default;
            previous[index++] = new BacktrackNode(current, cont);
        }
    }

    private record struct BacktrackNode(Node<TKey, TResult> Node, SequenceCheckpoint Checkpoint);

    private static PartialResult<TResult> FindBestResult(int index, BacktrackNode[] previous, ISequence<TKey> keys, SequenceCheckpoint startCont, int startConsumed)
    {
        while (index > 0)
        {
            var (node, cont) = previous[--index];
            if (node.HasResult)
            {
                cont.Rewind();
                ArrayPool<BacktrackNode>.Shared.Return(previous);
                return new PartialResult<TResult>(node.Result!, keys.Consumed - startConsumed);
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
        previous[index++] = new BacktrackNode(current, startCont);

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
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child, find the best value
            if (!current.ContainsKey(wrappedKey))
                return FindBestResult(index, previous, keys, startCont, 0).Success;

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current[wrappedKey];
            previous[index++] = new BacktrackNode(current, cont);
        }
    }

    public IReadOnlyList<IResultAlternative<TResult>> GetMany(ISequence<TKey> keys)
    {
        Node<TKey, TResult> current = this;
        var results = new List<IResultAlternative<TResult>>();

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging so return any values we have
            if (keys.IsAtEnd || current.Count == 0)
                return results;

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child in this node, return the results we have
            if (!current.ContainsKey(wrappedKey))
                return results;

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current[wrappedKey];
            if (current.HasResult && current.Result != null)
                results.Add(new SuccessResultAlternative<TResult>(current.Result, cont.Consumed, cont));
        }
    }

    public bool TryAdd(IReadOnlyList<TKey> keyList, TResult value)
    {
        if (!Editable)
            throw new TrieInsertException("Cannot insert new items into a Trie which has been locked");

        Node<TKey, TResult> current = this;
        foreach (var key in keyList)
            current = current.GetOrAddChild(key);

        if (current.TryAddResult(value))
        {
            SetPatternDepth(keyList.Count);
            return true;
        }

        return false;
    }

    // Node and RootNode IS-A Dictionary, and Dictionary has annotations to prevent nullable types
    // from being used as Keys. However, due to the generic nature of parsing, we may be parsing
    // a sequence of nullable types such as Tokens. For that reason, we wrap our key values in
    // a non-nullable struct ValueTuple<TKey> which should be free to allocate and cannot be null
    // (although the only internal value is a null pointer, so a by-value comparsion would be
    // against a null pointer which is treated like untyped memory). We use this equality comparer
    // to wrap up an IEqualityComparer<TKey> so we can pass it to the dictionary.
    private class WrappedEqualityComparer : IEqualityComparer<ValueTuple<TKey>>
    {
        public IEqualityComparer<TKey> _inner;

        public WrappedEqualityComparer(IEqualityComparer<TKey> inner)
        {
            _inner = inner;
        }

        public bool Equals(ValueTuple<TKey> x, ValueTuple<TKey> y)
            => _inner.Equals(x.Item1, y.Item1);

        public int GetHashCode([DisallowNull] ValueTuple<TKey> obj)
            => obj.Item1 == null ? 0 : _inner.GetHashCode(obj.Item1!);
    }
}
