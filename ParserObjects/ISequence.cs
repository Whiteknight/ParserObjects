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
}