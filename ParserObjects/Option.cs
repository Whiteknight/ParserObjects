using System;

namespace ParserObjects
{
    public struct Option<T>
    {
        private readonly T _value;

        public Option(T value)
        {
            _value = value;
            Success = true;
        }

        public bool Success { get; }

        public T Value
        {
            get
            {
                if (!Success)
                    throw new InvalidOperationException("This Option does not contain success data");
                return _value;
            }
        }

        public void Deconstruct(out bool success, out T value)
        {
            success = Success;
            value = _value;
        }
    }
}
