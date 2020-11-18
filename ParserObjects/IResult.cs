namespace ParserObjects
{
    public interface IResult
    {
        IParser Parser { get; }

        /// <summary>
        /// Returns true if the parse succeeded, false otherwise.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// The approximate location of the successful parse in the input sequence. On failure, this
        /// value is undefined and may show the location of the start of the attempt, the location at
        /// which failure occured, null, or some other value.
        /// </summary>
        Location Location { get; }

        string Message { get; }
    }
}