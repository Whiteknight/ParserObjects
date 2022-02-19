namespace ParserObjects.Earley;

public sealed class ParseStatistics : IParseStatistics
{
    public int NumberOfStates { get; set; }
    public int CreatedItems { get; set; }
    public int PredictedItems { get; set; }

    public int PredictedByCompletedNullable { get; set; }
    public int ScannedSuccess { get; set; }
    public int CompletedParentItem { get; set; }
    public int CompletedNullables { get; set; }
    public int DerivationCacheHit { get; set; }
    public int DerivationSingleSymbolShortCircuits { get; set; }
    public int ItemsWithZeroDerivations { get; set; }
    public int ItemsWithSingleDerivation { get; set; }
    public int ProductionRuleAttempts { get; set; }
    public int ProductionRuleSuccesses { get; set; }
}
