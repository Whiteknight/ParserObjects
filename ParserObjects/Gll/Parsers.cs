using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Gll
{
    public class AlternativesGllParser<TInput, TOutput> : IGllParser<TInput, TOutput>
    {
        private readonly IGllParser<TInput>[] _parsers;

        public AlternativesGllParser(params IGllParser<TInput, TOutput>[] parsers)
        {
            _parsers = parsers;
            Id = UniqueIntegerGenerator.GetNext();
        }

        public int Id { get; }

        public string Name => throw new NotImplementedException();

        public IEnumerable<IParser> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Parse(IState<TInput> state, IResultPromise results)
        {
            int count = _parsers.Length;
            // TODO: Instead of closing over count here, we can tell results.ExpectCount(N) and
            // have it automatically set IsComplete=true when the specified number of results is
            // reached.
            foreach (var parser in _parsers)
            {
                state.Schedule(parser).Then(match =>
                {
                    results.Add(match);
                    count--;
                    if (count == 0)
                        results.IsComplete = true;
                });
            }
        }

        public INamed SetName(string name)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class RuleGllParser<TInput, TOutput> : IGllParser<TInput, TOutput>
    {
        private readonly Func<IReadOnlyList<object>, TOutput> _combine;
        private readonly IReadOnlyList<IGllParser<TInput>> _parsers;

        public RuleGllParser(IReadOnlyList<IGllParser<TInput>> parsers, Func<IReadOnlyList<object>, TOutput> combine)
        {
            _combine = combine;
            Id = UniqueIntegerGenerator.GetNext();
            _parsers = parsers;
        }

        public int Id { get; }

        public string Name => throw new NotImplementedException();

        public IEnumerable<IParser> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Parse(IState<TInput> state, IResultPromise results)
        {
            void Next(IState<TInput> thisState, int i, List<object> values)
            {
                if (i >= _parsers.Count)
                {
                    var finalResult = _combine(values);
                    results.AddSuccess(thisState, finalResult);
                    return;
                }

                var parser = _parsers[i];
                thisState.Advance().Schedule(parser).Then(match =>
                {
                    if (!match.Success)
                    {
                        results.Add(match);
                        return;
                    }

                    values.Add(match.GetObjectValue().Value);
                    Next(match.State as IState<TInput>, i + 1, values);
                });
            }

            var resultValues = new List<object>();
            Next(state, 0, resultValues);
        }

        public INamed SetName(string name)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class OptionalGllParser<TInput, TOutput> : IGllParser<TInput, TOutput>
    {
        private readonly IGllParser<TInput, TOutput> _inner;

        public OptionalGllParser(IGllParser<TInput, TOutput> inner)
        {
            _inner = inner;
            Id = UniqueIntegerGenerator.GetNext();
        }

        public int Id { get; }

        public string Name => throw new NotImplementedException();

        public IEnumerable<IParser> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Parse(IState<TInput> state, IResultPromise results)
        {
            state.Schedule(_inner).Then(match =>
            {
                if (match.Success)
                {
                    results.Add(match);
                    results.IsComplete = true;
                    return;
                }

                results.AddSuccess(state, default(TOutput));
                results.IsComplete = true;
                return;
            });
        }

        public INamed SetName(string name)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class PositiveLookaheadGllParser<TInput, TOutput> : IGllParser<TInput, TOutput>
    {
        private readonly IGllParser<TInput, TOutput> _inner;

        public PositiveLookaheadGllParser(IGllParser<TInput, TOutput> inner)
        {
            _inner = inner;
            Id = UniqueIntegerGenerator.GetNext();
        }

        public int Id { get; }

        public string Name => throw new NotImplementedException();

        public IEnumerable<IParser> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Parse(IState<TInput> state, IResultPromise results)
        {
            state.Schedule(_inner).Then(match =>
            {
                if (!match.Success)
                {
                    results.Add(match);
                    results.IsComplete = true;
                    return;
                }

                results.Add(match.WithState(state));
                results.IsComplete = true;
                return;
            });
        }

        public INamed SetName(string name)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => DefaultStringifier.ToString(this);
    }

    public class TransformGllParser<TInput, TMiddle, TOutput> : IGllParser<TInput, TOutput>
    {
        private readonly IGllParser<TInput, TMiddle> _inner;
        private readonly Func<TMiddle, TOutput> _transform;

        public TransformGllParser(IGllParser<TInput, TMiddle> inner, Func<TMiddle, TOutput> transform)
        {
            _inner = inner;
            _transform = transform;
            Id = UniqueIntegerGenerator.GetNext();
        }

        public int Id { get; }

        public string Name => throw new NotImplementedException();

        public IEnumerable<IParser> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Parse(IState<TInput> state, IResultPromise results)
        {
            state.Schedule(_inner).Then(match =>
            {
                if (!match.Success)
                {
                    results.Add(match);
                    return;
                }

                var value = match.GetValue<TMiddle>().Value;
                var transformed = _transform(value);
                results.Add((match.State as IState<TInput>).Success(transformed));
            });
        }

        public INamed SetName(string name)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
