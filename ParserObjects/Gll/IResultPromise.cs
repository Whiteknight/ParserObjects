using System;
using System.Collections.Generic;

namespace ParserObjects.Gll
{
    public interface IResultPromise
    {
        bool IsComplete { get; set; }

        void AddFailure(IState state, string error);

        void Add(IMatch value);

        void AddSuccess<TValue>(IState state, TValue value);

        IReadOnlyList<IMatch> AllValues { get; }
    }

    public interface IResultContinuation
    {
        void Then(Action<IMatch> listener);
    }

    public class ResultPromise<TInput> : IResultPromise, IResultContinuation
    {
        private readonly List<IMatch> _values;
        private readonly List<Action<IMatch>> _listeners;
        private readonly Action<Exception> _exceptionHandler;

        public ResultPromise(Action<Exception> exceptionHandler)
        {
            _values = new List<IMatch>();
            _listeners = new List<Action<IMatch>>();
            _exceptionHandler = exceptionHandler;
        }

        public bool IsSettled => _values.Count > 0;

        public IReadOnlyList<IMatch> AllValues => _values;

        public bool IsComplete { get; set; }

        public void Add(IMatch value)
        {
            _values.Add(value);
            foreach (var listener in _listeners)
                listener(value);
        }

        public void AddFailure(IState state, string error)
        {
            // TODO: Get the real location from the state
            Add(new FailureMatch<TInput>(state, error));
        }

        public void AddSuccess<TValue>(IState state, TValue value)
        {
            Add(new SuccessMatch<TInput, TValue>(state, value));
        }

        public void Then(Action<IMatch> listener)
        {
            void SafeListener(IMatch r)
            {
                try
                {
                    listener(r);
                }
                catch (Exception e)
                {
                    _exceptionHandler(e);
                }
            }

            _listeners.Add(SafeListener);
            foreach (var value in _values)
                SafeListener(value);
        }
    }
}
