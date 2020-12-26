namespace ParserObjects.Gll
{
    public interface IMatch
    {
        // TODO: Include location? sequence checkpoint?
        bool Success { get; }

        IState State { get; }

        IOption<object> GetObjectValue();

        IOption<TValue> GetValue<TValue>();

        string ErrorMessage { get; }

        IMatch WithState(IState state);
    }

    public class SuccessMatch<TInput, TOutput> : IMatch
    {
        private readonly TOutput _value;

        public SuccessMatch(IState state, TOutput value)
        {
            State = state;
            _value = value;
        }

        public bool Success => true;
        public IState State { get; }

        public IOption<TValue> GetValue<TValue>()
        {
            if (_value is TValue typed)
                return new SuccessOption<TValue>(typed);
            return FailureOption<TValue>.Instance;
        }

        public IOption<object> GetObjectValue()
        {
            return new SuccessOption<object>(_value);
        }

        public string ErrorMessage => string.Empty;

        public IMatch WithState(IState state) => new SuccessMatch<TInput, TOutput>(state, _value);
    }

    public class FailureMatch<TInput> : IMatch
    {
        public FailureMatch(IState state, string errorMessage)
        {
            State = state;
            ErrorMessage = errorMessage;
        }

        public bool Success => false;
        public IState State { get; }

        public IOption<TValue> GetValue<TValue>() => FailureOption<TValue>.Instance;

        public IOption<object> GetObjectValue() => FailureOption<object>.Instance;

        public IMatch WithState(IState state) => new FailureMatch<TInput>(state, ErrorMessage);

        public string ErrorMessage { get; }
    }
}
