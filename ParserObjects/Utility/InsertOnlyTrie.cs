using System;
using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Utility
{
    public class InsertOnlyTrie<TKey, TResult> : ITrie<TKey, TResult>
    {
        private readonly Node _root;

        public InsertOnlyTrie()
        {
            _root = new Node();
        }

        public InsertOnlyTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result)
        {
            var current = _root;
            foreach (var key in keys)
                current = current.GetOrAdd(key);

            current.TryAddResult(result);
            return this;
        }

        public IParseResult<TResult> Get(IEnumerable<TKey> keys)
        {
            var current = _root;
            foreach (var key in keys)
            {
                current = current.Get(key);
                if (current == null)
                    return new FailResult<TResult>();
            }

            return current.HasResult ? new SuccessResult<TResult>(current.Result, null) : (IParseResult<TResult>)new FailResult<TResult>();
        }

        public IParseResult<TResult> Get(ISequence<TKey> keys) => _root.Get(keys);

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

            public IParseResult<TResult> Get(ISequence<TKey> keys)
            {
                if (HasResult && _children.Count == 0)
                    return new SuccessResult<TResult>(Result, keys.CurrentLocation);

                var key = keys.GetNext();
                if (!_children.ContainsKey(key))
                {
                    keys.PutBack(key);
                    if (HasResult)
                        return new SuccessResult<TResult>(Result, keys.CurrentLocation);
                    return new FailResult<TResult>(keys.CurrentLocation);
                }

                var result = _children[key].Get(keys);
                if (result.Success)
                    return result;

                if (HasResult)
                    return new SuccessResult<TResult>(Result, keys.CurrentLocation);

                keys.PutBack(key);
                return new FailResult<TResult>(keys.CurrentLocation);
            }

            public Node GetOrAdd(TKey key)
            {
                if (_children.ContainsKey(key))
                    return _children[key];
                var newNode = new Node();
                _children.Add(key, newNode);
                return newNode;
            }

            public void TryAddResult(TResult result)
            {
                if (!HasResult)
                {
                    HasResult = true;
                    Result = result;
                    return;
                }

                if (Result.Equals(result))
                    return;

                throw new Exception("The result value has already been set for this input sequence");
            }
        }
    }
}
