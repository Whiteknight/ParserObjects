namespace ParserObjects
{
    public interface ISequenceStatistics
    {
        int ItemsRead { get; }
        int ItemsPeeked { get; }
        int Rewinds { get; }
        int RewindsToCurrentBuffer { get; }
        int BufferFills { get; }
        int CheckpointsCreated { get; }
    }
}
