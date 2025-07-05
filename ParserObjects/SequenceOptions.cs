using System.Text;

namespace ParserObjects;

public static class SequenceOptions
{
    /// <summary>
    /// The default size to use for buffering data inside a sequence. Notice that some sequences
    /// may maintain multiple separate buffers of this size. The amount of memory used therefore
    /// will be proportional to this value.
    /// </summary>
    public const int DefaultBufferSize = 1024;

    public static SequenceOptions<char> ForRegex(string pattern)
        => new SequenceOptions<char>
        {
            FileName = pattern,
            MaintainLineEndings = true
        };
}

public readonly record struct SequenceOptions<T>(
    string FileName,
    int BufferSize,
    T EndSentinel,
    bool MaintainLineEndings,
    Encoding? Encoding
)
{
    /// <summary>
    /// Gets a value indicating whether to normalize line endings to '\n'.
    /// </summary>
    public readonly bool NormalizeLineEndings => !MaintainLineEndings;

    /// <summary>
    /// Validate the values and set defaults where values are omitted.
    /// </summary>
    public SequenceOptions<T> Validate()
        => this with
        {
            BufferSize = BufferSize <= 0 ? SequenceOptions.DefaultBufferSize : BufferSize,
            FileName = FileName ?? string.Empty,
            Encoding = Encoding ?? Encoding.UTF8
        };
}
