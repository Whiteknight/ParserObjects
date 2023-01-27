using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Tries;

public class Node<TKey, TResult>
{
    private readonly Dictionary<ValueTuple<TKey>, Node<TKey, TResult>> _children;

    public Node()
    {
        _children = new Dictionary<ValueTuple<TKey>, Node<TKey, TResult>>();
        HasResult = false;
    }

    public bool HasResult { get; private set; }

    public TResult? Result { get; private set; }

    public static PartialResult<TResult> Get(Node<TKey, TResult> thisNode, ISequence<TKey> keys)
    {
        var current = thisNode;

        // The node, and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = new Stack<(Node<TKey, TResult> node, SequenceCheckpoint cont)>();
        var startCont = keys.Checkpoint();
        var startConsumed = keys.Consumed;
        previous.Push((current, startCont));

        PartialResult<TResult> FindBestResult()
        {
            while (previous.Count > 0)
            {
                var (node, cont) = previous.Pop();
                if (node.HasResult)
                {
                    cont.Rewind();
                    return new PartialResult<TResult>(node.Result!, keys.Consumed - startConsumed);
                }
            }

            // No node matched, so return failure
            startCont.Rewind();
            return new PartialResult<TResult>("Trie does not contain matching item");
        }

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging and can start looking for a value
            // to return.
            if (keys.IsAtEnd || current._children.Count == 0)
                return FindBestResult();

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child, find the best value
            if (!current._children.ContainsKey(wrappedKey))
                return FindBestResult();

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current._children[wrappedKey];
            previous.Push((current, cont));
        }
    }

    public static bool CanGet(Node<TKey, TResult> thisNode, ISequence<TKey> keys)
    {
        var current = thisNode;

        // The node, and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = new Stack<(Node<TKey, TResult> node, SequenceCheckpoint cont)>();
        var startCont = keys.Checkpoint();
        previous.Push((current, startCont));

        bool FindBestResult()
        {
            while (previous.Count > 0)
            {
                var (node, cont) = previous.Pop();
                if (node.HasResult)
                {
                    cont.Rewind();
                    return true;
                }
            }

            // No node matched, so return failure
            startCont.Rewind();
            return false;
        }

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging and can start looking for a value
            // to return.
            if (keys.IsAtEnd || current._children.Count == 0)
                return FindBestResult();

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child, find the best value
            if (!current._children.ContainsKey(wrappedKey))
                return FindBestResult();

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current._children[wrappedKey];
            previous.Push((current, cont));
        }
    }

    public static IReadOnlyList<IResultAlternative<TResult>> GetMany(Node<TKey, TResult> thisNode, ISequence<TKey> keys)
    {
        var current = thisNode;
        var results = new List<IResultAlternative<TResult>>();

        // The node, and the continuation checkpoint that allows parsing to continue
        // immediately afterwards.
        var previous = new Stack<(Node<TKey, TResult> node, SequenceCheckpoint cont)>();
        var startCont = keys.Checkpoint();
        previous.Push((current, startCont));

        while (true)
        {
            // Check degenerate cases first. If we're at the end of input or we're at a
            // leaf node in the trie, we're done digging so return any values we have
            if (keys.IsAtEnd || current._children.Count == 0)
                return results;

            // Get the next key. Wrap it in a ValueTuple to convince the compiler it's not
            // null.
            var key = keys.GetNext();
            var cont = keys.Checkpoint();
            var wrappedKey = new ValueTuple<TKey>(key);

            // If there's no matching child in this node, return the results we have
            if (!current._children.ContainsKey(wrappedKey))
                return results;

            // Otherwise push the current node and the checkpoint from which we can continue
            // parsing from onto the stack, and prepare for the next loop iteration.
            current = current._children[wrappedKey];
            if (current.HasResult && current.Result != null)
                results.Add(new SuccessResultAlternative<TResult>(current.Result, cont.Consumed, cont));
        }
    }

    public Node<TKey, TResult> GetOrAdd(TKey key)
    {
        Assert.ArgumentNotNull(key, nameof(key));
        var wrappedKey = new ValueTuple<TKey>(key);
        if (_children.ContainsKey(wrappedKey))
            return _children[wrappedKey];
        var newNode = new Node<TKey, TResult>();
        _children.Add(wrappedKey, newNode);
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
