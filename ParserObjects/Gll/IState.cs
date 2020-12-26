using System;

namespace ParserObjects.Gll
{
    public interface IState
    {
        int Depth { get; }
        string Id { get; }

        void Execute();

        void AddFailure(string message);

        IResultContinuation ResultContinuation { get; }
        IResultPromise ResultPromise { get; }

        IState Advance();

        ISequenceCheckpoint StartCheckpoint { get; }
    }

    public interface IState<TInput> : IState
    {
        ISequence<TInput> Input { get; }

        new IState<TInput> Advance();

        IResultContinuation Schedule(IGllParser<TInput> parser, Func<bool>? condition = null);
    }

    public static class StateExtensions
    {
        public static IMatch Success<TInput, TValue>(this IState<TInput> state, TValue value)
            => new SuccessMatch<TInput, TValue>(state, value);

        public static IMatch Failure<TInput>(this IState<TInput> state, string errorMessage)
            => new FailureMatch<TInput>(state, errorMessage);
    }

    public sealed class State<TInput> : IState<TInput>
    {
        private readonly ResultPromise<TInput> _results;
        private readonly Action<Exception> _handleException;
        private readonly Action<IState, Func<bool>> _schedule;
        private readonly IGllParser<TInput> _parser;

        private State(ISequence<TInput> input, ISequenceCheckpoint startCp, int depth, IGllParser<TInput> parser, ResultPromise<TInput> results, Action<Exception> handleException, Action<IState, Func<bool>> schedule)
        {
            Depth = depth;
            _parser = parser;
            Input = input;
            StartCheckpoint = startCp;
            _results = results;
            _handleException = handleException;
            _schedule = schedule;
            Id = $"{_parser.Id}:{startCp.Consumed}";
        }

        public string Id { get; }

        public int Depth { get; }

        public IResultContinuation ResultContinuation => _results;

        public IResultPromise ResultPromise => _results;

        public bool IsComplete => _results.IsComplete;

        public ISequence<TInput> Input { get; }
        public ISequenceCheckpoint StartCheckpoint { get; }

        public static State<TInput> New(ISequence<TInput> input, IGllParser<TInput> parser, Action<Exception> handleException, Action<IState, Func<bool>> schedule)
            => new State<TInput>(input, input.Checkpoint(), 0, parser, new ResultPromise<TInput>(handleException), handleException, schedule);

        public IState<TInput> Advance() => new State<TInput>(Input, Input.Checkpoint(), Depth, _parser, _results, _handleException, _schedule);

        IState IState.Advance() => Advance();

        public IResultContinuation Schedule(IGllParser<TInput> parser, Func<bool>? condition = null)
        {
            var nextState = CreateNestedStateForParser(parser);
            _schedule(nextState, condition);
            return nextState.ResultContinuation;
        }

        private State<TInput> CreateNestedStateForParser(IGllParser<TInput> parser)
        {
            var resultPromise = new ResultPromise<TInput>(_handleException);
            return new State<TInput>(Input, StartCheckpoint, Depth + 1, parser, resultPromise, _handleException, _schedule);
        }

        public void Execute()
        {
            StartCheckpoint.Rewind();
            _parser.Parse(this, _results);
        }

        public void AddFailure(string message)
        {
            _results.AddFailure(this, message);
        }

        // public IState Merge(State<TValue> other) => new State<TValue>(Math.Min(_startPosition, other._startPosition), Math.Max(_position, other._position), Depth, Parser, _engine);

        // public ResultPromise<TValue> Schedule(IGllParser parser, Func<bool> condition) => _engine.Schedule(Next(parser), condition);

        // TODO: Figure out success and failure results
    }
}
