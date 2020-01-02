namespace ParserObjects
{
    public struct FailResult<TOutput> : IParseResult<TOutput>
    {
        public FailResult(Location location)
        {
            Location = location;
        }

        public bool Success => false;
        public TOutput Value => default;
        public Location Location { get; }


        public IParseResult<object> Untype() => new FailResult<object>(Location);
    }
}