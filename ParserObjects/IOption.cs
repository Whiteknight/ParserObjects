using System;

namespace ParserObjects
{
    public interface IOption<T>
    {
        bool Success { get; }
        T Value { get; }

        T GetValueOrDefault(T defaultValue);

        bool Is(T value);
    }

    public class SuccessOption<T> : IOption<T>
    {
        public SuccessOption(T value)
        {
            Value = value;
        }

        public bool Success => true;

        public T Value { get; }

        public T GetValueOrDefault(T defaultValue) => Value;

        public bool Is(T value)
        {
            if (value == null)
                return Value == null;
            return value.Equals(Value);
        }
    }

    public class FailureOption<T> : IOption<T>
    {
        public static IOption<T> Instance { get; } = new FailureOption<T>();

        public bool Success => false;
        public T Value => throw new InvalidOperationException("This option does not contain a successful value");

        public T GetValueOrDefault(T defaultValue) => defaultValue;

        public bool Is(T value) => false;
    }
}
