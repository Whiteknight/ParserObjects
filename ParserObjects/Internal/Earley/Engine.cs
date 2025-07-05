using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects.Earley;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Earley;

/// <summary>
/// Engine for the Earley parser algorithm. Manages states and process, and returns the final
/// result.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public readonly struct Engine<TInput, TOutput>
{
    private readonly INonterminal<TInput, TOutput> _startSymbol;

    public Engine(INonterminal<TInput, TOutput> startSymbol)
    {
        if (NotNull(startSymbol).Productions.Count == 0)
            throw new GrammarException("The start symbol contains no valid productions");
        Debug.Assert(startSymbol.Productions.Any(p => p.Symbols.Count > 0), "Start symbol must have valid productions");
        _startSymbol = startSymbol;
    }

    public ParseResult<TOutput> Parse(IParseState<TInput> parseState)
    {
        NotNull(parseState);

        var stats = new ParseStatistics();

        var startCheckpoint = parseState.Input.Checkpoint();
        var states = new StateCollection(startCheckpoint);
        var currentState = GetInitialState(states, stats);
        var resultItems = new List<(Item Item, State State)>();

        while (currentState?.Items.Count > 0)
        {
            stats.NumberOfStates++;
            currentState.Checkpoint.Rewind();
            ParseCurrentState(parseState, states, currentState, resultItems, stats);
            currentState = states.MoveToNext();
        }

        return DeriveAllResults(stats, resultItems);
    }

    private static ParseResult<TOutput> DeriveAllResults(
        ParseStatistics stats,
        List<(Item Item, State State)> resultItems
    )
    {
        var derivationVisitor = new ItemDerivationVisitor(stats);
        var results = new List<Alternative<TOutput>>();
        for (int i = 0; i < resultItems.Count; i++)
        {
            var resultItem = resultItems[i];
            var derivations = derivationVisitor.GetDerivation(resultItem.Item);
            for (int j = 0; j < derivations.Count; j++)
            {
                Debug.Assert(derivations[j] is TOutput, "The derivation must have the correct type");
                var value = (TOutput)derivations[j];
                var result = Alternative<TOutput>.Ok(value, resultItem.State.Number, resultItem.State.Checkpoint);
                results.Add(result);
            }
        }

        return new ParseResult<TOutput>(results, stats);
    }

    private State GetInitialState(StateCollection states, ParseStatistics stats)
    {
        // Get the initial state from the states collection and initialize it with items from
        // the start symbol
        var initialState = states.InitialState;
        foreach (var production in _startSymbol.Productions)
        {
            var initialItem = new Item(production, initialState, initialState);
            stats.CreatedItems++;
            initialState.Add(initialItem);
        }

        return initialState;
    }

    private void ParseCurrentState(
        IParseState<TInput> parseState,
        StateCollection states,
        State currentState,
        List<(Item Item, State State)> resultItems,
        ParseStatistics stats
    )
    {
        // A list of productions which were started in this state and completed in this
        // state ("zero-length", "empty" or "nullable")
        var completedNullables = new Dictionary<IProduction, IList<Item>>();

        // Items may be added to the list as we traverse the list, so we cannot use an
        // enumerator here.
        for (int i = 0; i < currentState.Items.Count; i++)
        {
            var item = currentState.Items[i];

            if (item.AtEnd)
            {
                // This is a complete item. Advance items which are depending on it.
                Complete(currentState, item, completedNullables, stats);

                // If the completed item is a start item, add it to the list of possible
                // results.
                if (item.ParentState == states.InitialState && _startSymbol.Productions.Contains(item.Production))
                    resultItems.Add((item, currentState));
                continue;
            }

            // If the item is a non-terminal, predict what the state should look like and
            // add those predicted items to the current state.
            if (item.NextSymbolToMatch is INonterminal nonterminal)
            {
                Predict(currentState, item, nonterminal, completedNullables, stats);
                continue;
            }

            // If the item is terminal, see if we match, and add successful matches to the
            // next state.
            if (item.NextSymbolToMatch is IParser<TInput> terminal)
                Scan(currentState, item, terminal, states, parseState, stats);

            if (item.NextSymbolToMatch is IMultiParser<TInput> multiTerminal)
                Scan(currentState, item, multiTerminal, states, parseState, stats);
        }

        Debug.WriteLine(currentState.GetCompleteListing());
    }

    private static void Predict(
        State state,
        Item item,
        INonterminal nonterminal,
        Dictionary<IProduction, IList<Item>> completedNullables,
        ParseStatistics stats
    )
    {
        // Add prediction states
        var relevantCompletedNullableProductions = new List<IProduction>();
        foreach (var p in nonterminal.Productions)
        {
            var newItem = new Item(p, state, state);
            stats.CreatedItems++;
            if (completedNullables.ContainsKey(p))
                relevantCompletedNullableProductions.Add(p);
            if (state.Contains(newItem))
                continue;
            state.Add(newItem);
            stats.PredictedItems++;
        }

        // Aycock fix: If this item is waiting on a symbol which is already in the list of
        // nullables, we can simply advance it here
        if (relevantCompletedNullableProductions.Count > 0)
        {
            var aycockItem = state.Import(item.CreateNextItem(state));
            stats.CreatedItems++;
            var seenNullables = new HashSet<Item>();
            for (int i = 0; i < relevantCompletedNullableProductions.Count; i++)
            {
                var relevantProduction = relevantCompletedNullableProductions[i];
                var assocatedNullables = completedNullables[relevantProduction];
                for (int j = 0; j < assocatedNullables.Count; j++)
                {
                    var nullable = assocatedNullables[j];
                    seenNullables.Add(nullable);
                    aycockItem.Add(nullable);
                }
            }

            state.Add(aycockItem);
            stats.PredictedItems++;
            stats.PredictedByCompletedNullable++;
        }
    }

    private static (Result<object> Result, SequenceCheckpoint Continuation) TryParse(
        IParser<TInput> terminal,
        IParseState<TInput> parseState,
        SequenceCheckpoint stateCheckpoint
    )
    {
        var location = parseState.Input.CurrentLocation;
        var cached = parseState.Cache.Get<CachedParseResult>(terminal, location);
        if (cached.Success)
            return (cached.Value.Result, cached.Value.Continuation);

        var result = terminal.Parse(parseState);
        var continuation = result.Success ? parseState.Input.Checkpoint() : stateCheckpoint;
        parseState.Cache.Add(terminal, location, new CachedParseResult(result.AsObject(), continuation));
        return (result, continuation);
    }

    private static MultiResult<object> TryParse(IMultiParser<TInput> terminal, IParseState<TInput> parseState)
    {
        var location = parseState.Input.CurrentLocation;
        var cached = parseState.Cache.Get<MultiResult<object>>(terminal, location);
        if (cached.Success)
            return cached.Value;

        var result = terminal.Parse(parseState);
        parseState.Cache.Add(terminal, location, result);
        return result;
    }

    private static void Scan(
        State currentState,
        Item item,
        IParser<TInput> terminal,
        StateCollection states,
        IParseState<TInput> parseState,
        ParseStatistics stats
    )
    {
        var (result, continuation) = TryParse(terminal, parseState, currentState.Checkpoint);
        if (!result.Success)
            return;

        var nextState = states.GetAhead(result.Consumed, continuation);

        var newItem = item.CreateNextItem(nextState);
        stats.CreatedItems++;

        // Add the value to the new item's _derivations list. The value of a Terminal is the
        // returned value from the parser.
        newItem.SetTerminalValue(result.Value);
        nextState.Add(newItem);
        stats.ScannedSuccess++;

        if (result.Consumed > 0)
            currentState.Checkpoint.Rewind();
    }

    private static void Scan(
        State currentState,
        Item item,
        IMultiParser<TInput> terminal,
        StateCollection states,
        IParseState<TInput> parseState,
        ParseStatistics stats
    )
    {
        var result = TryParse(terminal, parseState);
        for (int i = 0; i < result.Results.Count; i++)
        {
            var successResult = result.Results[i];
            if (!successResult.Success)
                continue;

            var nextState = states.GetAhead(successResult.Consumed, successResult.Continuation);

            var newItem = item.CreateNextItem(nextState);
            stats.CreatedItems++;

            // Add the value to the new item's _derivations list. The value of a Terminal is the
            // returned value from the parser.
            newItem.SetTerminalValue(successResult.Value);
            nextState.Add(newItem);
            stats.ScannedSuccess++;
        }

        currentState.Checkpoint.Rewind();
    }

    private static void AddCompletedNullable(
        Dictionary<IProduction, IList<Item>> completedNullables,
        Item item,
        IProduction production,
        ParseStatistics stats
    )
    {
        if (!completedNullables.ContainsKey(production))
            completedNullables[production] = new List<Item>();
        completedNullables[production].Add(item);
        stats.CompletedNullables++;
    }

    private static void Complete(
        State state,
        Item item,
        Dictionary<IProduction, IList<Item>> completedNullables,
        ParseStatistics stats
    )
    {
        // item is complete. Go through Items in the parent state which were waiting for item
        // and advance them.

        // For every item in the parent state which is incomplete and is waiting on a
        // non-terminal for which this production is an alternative, create a new Item in the
        // current state which is the same as the parent item, but advanced past the
        // non-terminal.
        var parentStateItemsToAdvance = item.ParentState.GetLiveItemsWaitingForProduction(item.Production);
        for (int i = 0; i < parentStateItemsToAdvance.Count; i++)
        {
            var parentItem = parentStateItemsToAdvance[i];
            var newItem = state.Import(parentItem.CreateNextItem(state));
            stats.CreatedItems++;

            // Add the item to the newItem's _derivations list. When we derive the value, we
            // need to recurse from newItem->item to get that part of it.
            newItem.Add(item);
            stats.CompletedParentItem++;

            // We have advanced newItem. Now check to see if any of the productions in newItem
            // have a completed nullable next. If so, add the necessary completed nullable items
            // to the current state so the next loop through will complete those too.
            if (newItem.ValueSymbol is INonterminal nonterminal)
            {
                foreach (var production in nonterminal.Productions)
                {
                    if (!completedNullables.ContainsKey(production))
                        continue;
                    var completedNullablesForThisProduction = completedNullables[production];
                    for (int j = 0; j < completedNullablesForThisProduction.Count; j++)
                        newItem.Add(completedNullablesForThisProduction[j]);
                }
            }
        }

        parentStateItemsToAdvance.Return();

        // If the parent state is the current state, that means the item is zero-length and
        // is considered "nullable"
        if (item.ParentState == state)
            AddCompletedNullable(completedNullables, item, item.Production, stats);
    }

    private sealed record CachedParseResult(Result<object> Result, SequenceCheckpoint Continuation);
}
