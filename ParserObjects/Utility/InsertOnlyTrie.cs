using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Utility
{
    /// <summary>
    /// Trie implementation which allows inserts of values but not updates of values. Once a value is
    /// inserted into the trie, it cannot be removed or modified.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class InsertOnlyTrie<TKey, TResult> : IInsertableTrie<TKey, TResult>
    {
        private readonly Node _root;
        private readonly List<IReadOnlyList<TKey>> _patterns;

        public InsertOnlyTrie()
        {
            _root = new Node();
            _patterns = new List<IReadOnlyList<TKey>>();
        }

        public IInsertableTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result)
        {
            Assert.ArgumentNotNull(keys, nameof(keys));
            Assert.ArgumentNotNull(result, nameof(result));
            var current = _root;
            var keyList = keys.ToList();
            foreach (var key in keyList)
                current = current.GetOrAdd(key);

            if (current.TryAddResult(result))
                _patterns.Add(keyList);
            return this;
        }

        public PartialResult<TResult> Get(ISequence<TKey> keys)
        {
            Assert.ArgumentNotNull(keys, nameof(keys));
            return Node.Get(_root, keys);
        }

        public IEnumerable<IReadOnlyList<TKey>> GetAllPatterns() => _patterns;

        private class Node
        {
            private readonly Dictionary<ValueTuple<TKey>, Node> _children;

            public Node()
            {
                _children = new Dictionary<ValueTuple<TKey>, Node>();
                HasResult = false;
            }

            public bool HasResult { get; private set; }

            public TResult? Result { get; private set; }

            public static PartialResult<TResult> Get(Node thisNode, ISequence<TKey> keys)
            {
                var startLocation = keys.CurrentLocation;
                var current = thisNode;

                // The node, and the continuation checkpoint that allows parsing to continue
                // immediately afterwards.
                var previous = new Stack<(Node node, ISequenceCheckpoint cont)>();
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
                            return new PartialResult<TResult>(node.Result!, keys.Consumed - startConsumed, startLocation);
                        }
                    }

                    // No node matched, so return failure
                    startCont.Rewind();
                    return new PartialResult<TResult>("Trie does not contain matching item", startLocation);
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

            public Node GetOrAdd(TKey key)
            {
                Assert.ArgumentNotNull(key, nameof(key));
                var wrappedKey = new ValueTuple<TKey>(key);
                if (_children.ContainsKey(wrappedKey))
                    return _children[wrappedKey];
                var newNode = new Node();
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
    }

    [Serializable]
    public class TrieInsertException : Exception
    {
        public TrieInsertException()
        {
        }

        public TrieInsertException(string message) : base(message)
        {
        }

        public TrieInsertException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TrieInsertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
