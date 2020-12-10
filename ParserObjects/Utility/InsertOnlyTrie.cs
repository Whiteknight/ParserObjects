using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IPartialResult<TResult> Get(ISequence<TKey> keys)
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

            public static IPartialResult<TResult> Get(Node thisNode, ISequence<TKey> keys)
            {
                var startLocation = keys.CurrentLocation;
                var current = thisNode;
                // The node and the key we apply to that node
                var previous = new Stack<(Node node, TKey key)>();
                int consumed = 0;

                IPartialResult<TResult> FindBestResult()
                {
                    while (previous.Count > 0)
                    {
                        var (node, oldKey) = previous.Pop();
                        keys.PutBack(oldKey);
                        consumed--;
                        if (node.HasResult && node.Result != null)
                            return new SuccessPartialResult<TResult>(node.Result, consumed, startLocation);
                    }

                    // No node matched, so return failure
                    Debug.Assert(consumed == 0, "Just double-checking my math");
                    return new FailurePartialResult<TResult>("Trie does not contain matching item", startLocation);
                }

                while (true)
                {
                    if (keys.IsAtEnd)
                    {
                        if (current.HasResult && current.Result != null)
                            return new SuccessPartialResult<TResult>(current.Result, consumed, startLocation);
                        return FindBestResult();
                    }

                    // Quick degenerate case. We're at the final leaf of the trie, so return a
                    // value if we have it.
                    if (current.HasResult && current._children.Count == 0 && current.Result != null)
                        return new SuccessPartialResult<TResult>(current.Result, consumed, startLocation);

                    // Get the next key and push onto the stack
                    var key = keys.GetNext();
                    previous.Push((current, key));

                    // If we have more input to read, and if this node has a matching child, set that as the current node and jump
                    // back to the top of the loop
                    var wrappedKey = new ValueTuple<TKey>(key);
                    if (current._children.ContainsKey(wrappedKey))
                    {
                        consumed++;
                        current = current._children[wrappedKey];
                        continue;
                    }

                    // No matching child. So start looping over the nodes in the stack, looking
                    // for the first one with a value, and putting back all keys along the way.
                    consumed++;
                    return FindBestResult();
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

    [System.Serializable]
    public class TrieInsertException : System.Exception
    {
        public TrieInsertException()
        {
        }

        public TrieInsertException(string message) : base(message)
        {
        }

        public TrieInsertException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected TrieInsertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
