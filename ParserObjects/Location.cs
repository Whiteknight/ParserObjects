namespace ParserObjects
{
    /// <summary>
    /// An approximate description of the location in the data source where an item is located. Notice
    /// that some types of input may not make this information precisely knowable.
    /// </summary>
    public class Location
    {
        public Location(string fileName, int line, int column)
        {
            FileName = fileName;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// The name of the file, if the input source is a file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The line number, if the input is on multiple lines
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// The index of the item in the current file and line
        /// </summary>
        public int Column { get; }

        public override string ToString() 
            => !string.IsNullOrEmpty(FileName) ? $"File {FileName} at Line {Line} Column {Column}" : $"Line {Line} Column {Column}";
    }
}