using System;
using ParserObjects.Sequences;

namespace ParserObjects
{
    public interface ISequence<T>
    {
        void PutBack(T value);
        T GetNext();

        T Peek();
        Location CurrentLocation { get; }
        bool IsAtEnd { get; }
    }

    public static class SequenceExtensions
    {
        public static ISequence<TOutput> Map<TInput, TOutput>(this ISequence<TInput> input, Func<TInput, TOutput> map)
        {
            return new MapSequence<TInput, TOutput>(input, map);
        }
    }
}