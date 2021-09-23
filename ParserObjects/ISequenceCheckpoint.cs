namespace ParserObjects
{
    /// <summary>
    /// A snapshot of a sequence at a specific point. Can be used to return the sequence to that
    /// point.
    /// </summary>
    public interface ISequenceCheckpoint
    {
        /// <summary>
        /// Return the sequence to the state it was when the checkpoint was taken.
        /// </summary>
        void Rewind();

        // How many input items are consumed at the point of this checkpoint
        int Consumed { get; }

        Location Location { get; }
    }
}
