namespace ParserObjects;

/// <summary>
/// Contains statistics information about the sequence. This information is a snapshot and will not
/// update in-place as the sequence continues to be used.
/// </summary>
/// <param name="ItemsRead"></param>
/// <param name="ItemsPeeked"></param>
/// <param name="ItemsGenerated"></param>
/// <param name="Rewinds"></param>
/// <param name="RewindsToCurrentBuffer"></param>
/// <param name="BufferRefills"></param>
/// <param name="CheckpointsCreated"></param>
public readonly record struct SequenceStatistics(
    int ItemsRead,
    int ItemsPeeked,
    int ItemsGenerated,
    int Rewinds,
    int RewindsToCurrentBuffer,
    int BufferRefills,
    int CheckpointsCreated
);
