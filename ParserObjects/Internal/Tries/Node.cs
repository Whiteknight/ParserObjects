using System;
using System.Buffers;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Tries;

public class Node<TKey, TResult> : Dictionary<ValueTuple<TKey>, Node<TKey, TResult>>
{
    /* I generally don't like concrete inheritance, but in this case it's an optimization to
     * make the node be a class IS-A Dictionary. If we made it a struct there would be the same
     * number of allocations except we wouldn't be able to have reference behavior for setting
     * HasResult, Result and MaxDepth.
     */

    public Node()
    {
        HasResult = false;
        Result = default;
        MaxDepth = 0;
    }

    public bool HasResult { get; private set; }

    public TResult? Result { get; private set; }

    // Maximum depth of the longest pattern in the Trie. This value is only set on the root node
    public int MaxDepth { get; private set; }

    /* We call these static Get method variants instead of calling Get instance methods on the nodes
     * because it saves us a lot of recursion and we can use a rented array buffer for the stack.
     * Overall it makes things a bit nicer.
     */

    public static PartialResult<TResult> Get(Node<TKey, TResult> root, ISequence<TKey> keys)
    {
        var current = root;
        int maxDepth = root.MaxDepth;

        // The node, and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = ArrayPool<(Node<TKey, TResult> node, SequenceCheckpoint cont)>.Shared.Rent(maxDepth);
        var index = 0;
        var startCont = keys.Checkpoint();
        var startConsumed = keys.Consumed;
        previous[index++] = (current, startCont);

        PartialResult<TResult> FindBestResult()
        {
            while (index > 0)
            {
                var (node, cont) = previous[--index];
                if (node.HasResult)
                {
                    cont.Rewind();
                    ArrayPool<(Node<TKey, TResult> node, SequenceCheckpoint cont)>.Shared.Return(previous);
                    return new PartialResult<TResult>(node.Result!, keys.Consumed - startConsumed);
                }
            }

            // No node matched, so return failure
            startCont.Rewind();
            ArrayPool<(Node<TKey, TResult> node, SequenceCheckpoint cont)>.Shared.Return(previous);
            return new PartialResult<TResult>("Trie does not contain matching item");
        }

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging and can start looking for a value
            // to return.
            if (keys.IsAtEnd || current.Count == 0)
                return FindBestResult();

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child, find the best value
            if (!current.ContainsKey(wrappedKey))
                return FindBestResult();

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current[wrappedKey];
            previous[index++] = (current, cont);
        }
    }

    public static bool CanGet(Node<TKey, TResult> root, ISequence<TKey> keys)
    {
        var current = root;
        int maxDepth = root.MaxDepth;

        // The node, and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = ArrayPool<(Node<TKey, TResult> node, SequenceCheckpoint cont)>.Shared.Rent(maxDepth);
        var index = 0;
        var startCont = keys.Checkpoint();
        previous[index++] = (current, startCont);

        bool FindBestResult()
        {
            while (index > 0)
            {
                var (node, cont) = previous[--index];
                if (node.HasResult)
                {
                    cont.Rewind();
                    ArrayPool<(Node<TKey, TResult> node, SequenceCheckpoint cont)>.Shared.Return(previous);
                    return true;
                }
            }

            // No node matched, so return failure
            startCont.Rewind();
            ArrayPool<(Node<TKey, TResult> node, SequenceCheckpoint cont)>.Shared.Return(previous);
            return false;
        }

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging and can start looking for a value
            // to return.
            if (keys.IsAtEnd || current.Count == 0)
                return FindBestResult();

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child, find the best value
            if (!current.ContainsKey(wrappedKey))
                return FindBestResult();

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current[wrappedKey];
            previous[index++] = (current, cont);
        }
    }

    public static IReadOnlyList<IResultAlternative<TResult>> GetMany(Node<TKey, TResult> root, ISequence<TKey> keys)
    {
        var current = root;
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

    public Node<TKey, TResult> GetOrAddChild(TKey key)
    {
        Assert.ArgumentNotNull(key, nameof(key));
        var wrappedKey = new ValueTuple<TKey>(key);
        if (ContainsKey(wrappedKey))
            return this[wrappedKey];
        var newNode = new Node<TKey, TResult>();
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

    public void SetPatternDepth(int depth)
    {
        if (depth > MaxDepth)
            MaxDepth = depth;
    }
}
