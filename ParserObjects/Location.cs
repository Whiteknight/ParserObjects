namespace ParserObjects;

/// <summary>
/// A description of the location in the data source where an item is located. Notice that not all
/// sequences are able to communicate all pieces of data.
/// </summary>
public readonly record struct Location(string FileName, int Line, int Column)
{
    public override string ToString()
        => !string.IsNullOrEmpty(FileName) ? $"File {FileName} at Line {Line} Column {Column}" : $"Line {Line} Column {Column}";
}
