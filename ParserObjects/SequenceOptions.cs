using System.Text;

namespace ParserObjects;

public static class SequenceOptions
{
    public const int DefaultBufferSize = 1024;
}

public struct SequenceOptions<T>
{
    /// <summary>
    /// Gets or sets the name of the file to read from.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the size of the internal buffer, if used. Defaults to DefaultBufferSize.
    /// </summary>
    public int BufferSize { get; set; }

    /// <summary>
    /// Gets or sets the value of the end sentinel to use if an attempt is made to read past the
    /// end of the source data.
    /// </summary>
    public T EndSentinel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to maintain the original line-endings.
    /// </summary>
    public bool MaintainLineEndings { get; set; }

    /// <summary>
    /// Gets or sets the encoding to use when bytes need to be decoded to characters.
    /// </summary>
    public Encoding? Encoding { get; set; }

    /// <summary>
    /// Gets a value indicating whether to normalize line endings to '\n'.
    /// </summary>
    public bool NormalizeLineEndings => !MaintainLineEndings;

    public void Validate()
    {
        if (BufferSize <= 0)
            BufferSize = SequenceOptions.DefaultBufferSize;
        if (FileName == null)
            FileName = string.Empty;
        if (Encoding == null)
            Encoding = Encoding.UTF8;
    }
}
