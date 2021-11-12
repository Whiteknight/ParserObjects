namespace ParserObjects
{
    public interface IResultFactory<TOutput>
    {
        IResult<TOutput> Success(TOutput value, Location? location = null);

        IResult<TOutput> Failure(string errorMessage, Location? location = null);
    }
}
