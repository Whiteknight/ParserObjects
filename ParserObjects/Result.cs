using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    public record Result<TValue>(IParser Parser, bool Success, TValue Value, Location Location, string Message) : IResult<TValue>
    {
        object IResult.Value => Value;

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