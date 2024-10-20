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

    /// <summary>
    /// Validate the values and set defaults where values are omitted.
    /// </summary>
    public void Validate()
    {
        if (BufferSize <= 0)
            BufferSize = SequenceOptions.DefaultBufferSize;
        FileName ??= string.Empty;
        Encoding ??= Encoding.UTF8;
    }
}
