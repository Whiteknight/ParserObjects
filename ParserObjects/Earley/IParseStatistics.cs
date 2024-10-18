namespace ParserObjects.Earley;

/// <summary>
/// Parse statistics for the Early parser. Used to check complexity and assist with optimizations.
/// </summary>
public interface IParseStatistics
{
    int NumberOfStates { get; }
    int CreatedItems { get; }
    int CompletedNullables { get; }
    int CompletedParentItem { get; }
    int DerivationCacheHit { get; }
    int DerivationSingleSymbolShortCircuits { get; }
    int ItemsWithSingleDerivation { get; }
    int ItemsWithZeroDerivations { get; }
    int PredictedItems { get; }
    int PredictedByCompletedNullable { get; }
    int ProductionRuleAttempts { get; }
    int ProductionRuleSuccesses { get; }
    int ScannedSuccess { get; }
}
