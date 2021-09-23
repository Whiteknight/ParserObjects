using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Holds classes and information related to caching parsers.
    /// </summary>
    public static class Cache
    {
        private struct CachedParseResult<T>
        {
            public CachedParseResult(IResult<T> result, ISequenceCheckpoint continuation)
            {
                Result = result;
                Continuation = continuation;
            }

            public IResult<T> Result { get; }
            public ISequenceCheckpoint Continuation { get; }
        }

        private struct CachedParseResult
        {
            public CachedParseResult(IResult result, ISequenceCheckpoint continuation)
            {
                Result = result;
                Continuation = continuation;
            }

            public IResult Result { get; }
            public ISequenceCheckpoint Continuation { get; }
        }

        private record ParserLocationCacheKey(IParser Parser, Location Location) : ICacheable;

        /// <summary>
        /// A parser which caches an IResult without an explicit output type.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        public class NoOutputParser<TInput> : IParser<TInput>
        {
            public NoOutputParser(IParser<TInput> inner)
            {
                Inner = inner;
                Name = string.IsNullOrEmpty(inner.Name) ? string.Empty : $"{inner.Name}.Cache";
            }

            public string Name { get; set; }

            public IParser<TInput> Inner { get; }

            public override string ToString() => Inner.ToString() ?? string.Empty;

            public IEnumerable<IParser> GetChildren() => new[] { Inner };

            public IResult Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var key = new ParserLocationCacheKey(Inner, state.Input.CurrentLocation);
                var cached = state.Cache.Get<CachedParseResult>(key);
                if (!cached.Success)
                {
                    var result = Inner.Parse(state);
                    state.Cache.Add(key, new CachedParseResult(result, state.Input.Checkpoint()));
                    return result;
                }

                var entry = cached.Value;
                var cachedResult = entry.Result;
                if (!cachedResult.Success)
                    return cachedResult;

                var continuation = entry.Continuation;
                continuation.Rewind();

                return cachedResult;
            }
        }

        /// <summary>
        /// A parser that caches IResult<typeparamref name="TOutput"/>.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        public class OutputParser<TInput, TOutput> : IParser<TInput, TOutput>
        {
            public OutputParser(IParser<TInput, TOutput> inner)
            {
                Inner = inner;
                Name = string.IsNullOrEmpty(inner.Name) ? string.Empty : $"{inner.Name}.Cache";
            }

            public string Name { get; set; }

            public IParser<TInput, TOutput> Inner { get; }

            public override string ToString() => Inner.ToString() ?? string.Empty;

            public IEnumerable<IParser> GetChildren() => new[] { Inner };

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var key = new ParserLocationCacheKey(Inner, state.Input.CurrentLocation);
                var cached = state.Cache.Get<CachedParseResult<TOutput>>(key);
                if (!cached.Success)
                {
                    var result = Inner.Parse(state);
                    state.Cache.Add(key, new CachedParseResult<TOutput>(result, state.Input.Checkpoint()));
                    return result;
                }

                var entry = cached.Value;
                var cachedResult = entry.Result;
                if (!cachedResult.Success)
                    return cachedResult;

                var continuation = entry.Continuation;
                continuation.Rewind();

                return cachedResult;
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);
        }

        /// <summary>
        /// A parser which caches IMultiResult<typeparamref name="TOutput"/>.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        public class MultiParser<TInput, TOutput> : IMultiParser<TInput, TOutput>
        {
            public MultiParser(IMultiParser<TInput, TOutput> inner)
            {
                Inner = inner;
                Name = string.IsNullOrEmpty(inner.Name) ? string.Empty : $"{inner.Name}.Cache";
            }

            public string Name { get; set; }

            public IMultiParser<TInput, TOutput> Inner { get; }

            public override string ToString() => Inner.ToString() ?? string.Empty;

            public IEnumerable<IParser> GetChildren() => new[] { Inner };

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var key = new ParserLocationCacheKey(Inner, state.Input.CurrentLocation);
                var cached = state.Cache.Get<IMultiResult<TOutput>>(key);
                if (cached.Success)
                    return cached.Value;

                var result = Inner.Parse(state);
                state.Cache.Add(key, result);
                return result;
            }
        }
    }
}
