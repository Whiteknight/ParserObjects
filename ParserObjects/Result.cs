using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    public struct Result<TValue> : IResult
    {
        public Result(IParser parser, bool success, TValue value, Location location, string message)
        {
            Parser = parser;
            Success = success;
            Value = value;
            Location = location;
            Message = message;
        }

        public bool Success { get; }

        public TValue Value { get; }

        public Location Location { get; }

        public string Message { get; }

        public IParser Parser { get; }

        public Result<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            if (!Success)
                return new Result<TOutput>(Parser, false, default, Location, Message);
            var newValue = transform(Value);
            return new Result<TOutput>(Parser, true, newValue, Location, Message);
        }

        public override string ToString()
        {
            string parserName = "";
            if (Parser != null)
                parserName = (!string.IsNullOrEmpty(Parser.Name) ? Parser.Name : Parser.GetType().Name) + " ";
            var status = Success ? "Ok" : "Fail";
            var message = string.IsNullOrEmpty(Message) ? "" : ": " + Message;
            var location = Location == null ? "" : " at " + Location.ToString();
            return parserName + status + message + location;
        }

        public void Deconstruct(out bool success, out TValue value)
        {
            success = Success;
            value = Value;
        }

        public void Deconstruct(out bool success, out TValue value, out Location location)
        {
            success = Success;
            value = Value;
            location = Location;
        }

        /// <summary>
        /// Transforms the Value of the result to type object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public Result<object> Untype()
            => new Result<object>(Parser, Success, Value, Location, Message);

        public Result<TValue> WithError(string error)
            => new Result<TValue>(Parser, Success, Value, Location, error);

        public Result<TValue> WithError(Func<string, string> mutateError)
            => new Result<TValue>(Parser, Success, Value, Location, mutateError?.Invoke(Message) ?? Message);
    }
}