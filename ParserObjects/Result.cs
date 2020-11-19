using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    public class Result<TValue> : IResult<TValue>
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

        object IResult.Value => Value;

        public Location Location { get; }

        public string Message { get; }

        public IParser Parser { get; }

        public IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
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
    }
}