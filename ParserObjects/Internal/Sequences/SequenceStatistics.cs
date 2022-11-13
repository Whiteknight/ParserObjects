namespace ParserObjects.Internal.Sequences;

public struct SequenceStatistics : ISequenceStatistics
{
    public int ItemsRead { get; set; }
    public int ItemsPeeked { get; set; }
    public int Rewinds { get; set; }
    public int RewindsToCurrentBuffer { get; set; }
    public int BufferFills { get; set; }
    public int CheckpointsCreated { get; set; }
}
