using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects
{
    public interface ITrie<TKey, TResult>
    {
        InsertOnlyTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result);
        IParseResult<TResult> Get(IEnumerable<TKey> keys);
        IParseResult<TResult> Get(ISequence<TKey> keys);
    }
}