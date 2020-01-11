namespace ParserObjects
{
    public class Location
    {
        public Location(string fileName, int line, int column)
        {
            FileName = fileName;
            Line = line;
            Column = column;
        }

        public string FileName { get; }

        public int Line { get; }

        public int Column { get; }

        public override string ToString() 
            => !string.IsNullOrEmpty(FileName) ? $"File {FileName} at Line {Line} Column {Column}" : $"Line {Line} Column {Column}";
    }
}