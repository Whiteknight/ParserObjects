namespace ParserObjects;

/// <summary>
/// An approximate description of the location in the data source where an item is located. Notice
/// that some types of input may not make this information precisely knowable.
/// </summary>
public record struct Location(string FileName, int Line, int Column)
{
    public override string ToString()
        => !string.IsNullOrEmpty(FileName) ? $"File {FileName} at Line {Line} Column {Column}" : $"Line {Line} Column {Column}";
}
