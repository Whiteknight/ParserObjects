namespace ParserObjects.Sequences
{
    public class SequenceStatistics : ISequenceStatistics
    {
        public int ItemsRead { get; set; }
        public int ItemsPeeked { get; set; }
        public int Rewinds { get; set; }
        public int RewindsToCurrentBuffer { get; set; }
        public int BufferFills { get; set; }
        public int CheckpointsCreated { get; set; }

        public ISequenceStatistics Snapshot()
        {
            return new SequenceStatistics
            {
                ItemsRead = ItemsRead,
                ItemsPeeked = ItemsPeeked,
                Rewinds = Rewinds,
                RewindsToCurrentBuffer = RewindsToCurrentBuffer,
                BufferFills = BufferFills,
                CheckpointsCreated = CheckpointsCreated
            };
        }
    }
}
