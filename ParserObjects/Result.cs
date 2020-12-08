using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Parser result object which holds the result value and helpful metadata
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public record Result<TValue>(IParser Parser, bool Success, TValue Value, Location Location, string Message, int Consumed) : IResult<TValue>
    {
        object IResult.Value => Value;

        public IResult<TOutput> Transform<TOutput>(Func<TValue, TOutput> transform)
        {
            Assert.ArgumentNotNull(transform, nameof(transform));
            if (!Success)
                return new Result<TOutput>(Parser, false, default, Location, Message, Consumed);
            var newValue = transform(Value);
            return new Result<TOutput>(Parser, true, newValue, Location, Message, Consumed);
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
