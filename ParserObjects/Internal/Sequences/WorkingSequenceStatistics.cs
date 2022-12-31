namespace ParserObjects.Internal.Sequences;

// A mutable version of SequenceStatistics that a sequence can use to accumulate statistics.
public struct WorkingSequenceStatistics
{
    public int ItemsRead { get; set; }
    public int ItemsPeeked { get; set; }
    public int ItemsGenerated { get; set; }
    public int Rewinds { get; set; }
    public int RewindsToCurrentBuffer { get; set; }
    public int BufferFills { get; set; }
    public int CheckpointsCreated { get; set; }

    public SequenceStatistics Snapshot()
    {
        return new SequenceStatistics(ItemsRead, ItemsPeeked, ItemsGenerated, Rewinds, RewindsToCurrentBuffer, BufferFills, CheckpointsCreated);
    }
}
