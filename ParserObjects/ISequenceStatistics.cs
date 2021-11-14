namespace ParserObjects
{
    /// <summary>
    /// A snapshot of statistical information from an input sequence. Not all sequences will
    /// implement all statistic values.
    /// </summary>
    public interface ISequenceStatistics
    {
        /// <summary>
        /// Gets the number of times .GetNext() is called.
        /// </summary>
        int ItemsRead { get; }

        /// <summary>
        /// Gets the number of times .Peek() is called.
        /// </summary>
        int ItemsPeeked { get; }

        /// <summary>
        /// Gets the number of times a sequence checkpoint is invoked.
        /// </summary>
        int Rewinds { get; }

        /// <summary>
        /// Gets the number of times a sequence checkpoint rewind occured within the current
        /// buffer.
        /// </summary>
        int RewindsToCurrentBuffer { get; }

        /// <summary>
        /// Gets the number of times a buffer was refilled.
        /// </summary>
        int BufferFills { get; }

        /// <summary>
        /// Gets the number of checkpoints created.
        /// </summary>
        int CheckpointsCreated { get; }
    }
}
