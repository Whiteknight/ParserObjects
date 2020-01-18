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
        /// If the input source is a file, this is the name of the file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// If the input source has multiple lines, this is the line number
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