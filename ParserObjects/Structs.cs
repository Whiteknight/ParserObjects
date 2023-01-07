namespace ParserObjects;

public record struct RightApplyArguments<TOutput, TMiddle>(TOutput Left, TMiddle Middle, TOutput Right);
