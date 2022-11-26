namespace ParserObjects;

public readonly record struct SequenceStatistics(int ItemsRead, int ItemsPeeked, int Rewinds, int RewindsToCurrentBuffer, int BufferRefills, int CheckpointsCreated);
