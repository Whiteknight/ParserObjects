namespace ParserObjects.Earley;

public interface IParseStatistics
{
    int NumberOfStates { get; }
    int CreatedItems { get; }
    int CompletedNullables { get; }
    int CompletedParentItem { get; }
    int DerivationCacheHit { get; }
    int DerivationSingleSymbolShortcircuits { get; }
    int ItemsWithSingleDerivation { get; }
    int ItemsWithZeroDerivations { get; }
    int PredictedItems { get; }
    int PredictedByCompletedNullable { get; }
    int ProductionRuleAttempts { get; }
    int ProductionRuleSuccesses { get; }
    int ScannedSuccess { get; }
}
