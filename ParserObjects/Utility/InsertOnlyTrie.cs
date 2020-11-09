using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Utility
{
    /// <summary>
    /// Trie implementation which allows inserts of values but not updates of values. Once a value is
    /// inserted into the trie, it cannot be removed or modified
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
            var current = _root;
            var keyList = keys.ToList();
            foreach (var key in keyList)
                current = current.GetOrAdd(key);

            if (current.TryAddResult(result))
                _patterns.Add(keyList);
            return this;
        }

        public IResult<TResult> Get(IEnumerable<TKey> keys)
        {
            Assert.ArgumentNotNull(keys, nameof(keys));
            var current = _root;
            foreach (var key in keys)
            {
                current = current.Get(key);
                if (current == null)
                    return Result.Fail<TResult>();
            }

            return Result.New(current.HasResult, current.Result);
        }

        public IResult<TResult> Get(ISequence<TKey> keys)
        {
            Assert.ArgumentNotNull(keys, nameof(keys));
            return _root.Get(keys);
        }

        public IEnumerable<IReadOnlyList<TKey>> GetAllPatterns() => _patterns;

        private class Node
        {
            private readonly Dictionary<TKey, Node> _children;

            public Node()
            {
                _children = new Dictionary<TKey, Node>();
                HasResult = false;
            }

            public bool HasResult { get; private set; }

            public TResult Result { get; private set; }

            public Node Get(TKey key)
            {
                if (_children.ContainsKey(key))
                    return _children[key];
                return null;
            }

            public IResult<TResult> Get(ISequence<TKey> keys)
            {
                if (HasResult && _children.Count == 0)
                    return ParserObjects.Result.Success(Result, keys.CurrentLocation);

                var key = keys.GetNext();
                if (!_children.ContainsKey(key))
                {
                    keys.PutBack(key);
                    return ParserObjects.Result.New(HasResult, Result, keys.CurrentLocation);
                }

                var result = _children[key].Get(keys);
                if (result.Success)
                    return result;

                if (HasResult)
                    return ParserObjects.Result.Success(Result, keys.CurrentLocation);

                keys.PutBack(key);
                return ParserObjects.Result.Fail<TResult>(keys.CurrentLocation);
            }

            public Node GetOrAdd(TKey key)
            {
                if (_children.ContainsKey(key))
                    return _children[key];
                var newNode = new Node();
                _children.Add(key, newNode);
                return newNode;
            }

            public bool TryAddResult(TResult result)
            {
                if (!HasResult)
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
        public TrieInsertException() { }
        public TrieInsertException(string message) : base(message) { }
        public TrieInsertException(string message, System.Exception inner) : base(message, inner) { }
        protected TrieInsertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
