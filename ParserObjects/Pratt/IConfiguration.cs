using System;

namespace ParserObjects.Pratt
{
    public interface IConfiguration<TInput, TOutput>
    {
        /// <summary>
        /// Add a parselet.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="matcher"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        IConfiguration<TInput, TOutput> Add<TValue>(IParser<TInput, TValue> matcher, Action<IParseletConfiguration<TInput, TValue, TOutput>> setup);

        IConfiguration<TInput, TOutput> Add(IParser<TInput, TOutput> matcher);

        IConfiguration<TInput, TOutput> Reference(IParser parser);
    }
}
