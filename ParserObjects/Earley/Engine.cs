using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects;
using ParserObjects.Utility;

namespace ParserObjects.Earley
{
    // Engine for the Earley parser algorithm. Manages states and process, and returns the final
    // result
    public class Engine<TInput, TOutput>
    {
        private readonly INonterminal<TInput, TOutput> _startSymbol;

        public Engine(INonterminal<TInput, TOutput> startSymbol)
        {
            Assert.ArgumentNotNull(startSymbol, nameof(startSymbol));
            if (startSymbol.Productions.Count == 0)
                throw new GrammarException("The start symbol contains no valid productions");
            if (startSymbol.Productions.All(p => p.Symbols.Count == 0))
                throw new GrammarException("The start symbol productions contain no symbols");

            _startSymbol = startSymbol;
        }

        public ParseResult<TOutput> Parse(IParseState<TInput> parseState)
        {
            Assert.ArgumentNotNull(parseState, nameof(parseState));

            var stats = new ParseStatistics();

            var startCheckpoint = parseState.Input.Checkpoint();
            var states = new StateCollection(startCheckpoint);
            var initialState = GetInitialState(states, stats);
            var currentState = initialState;
            var resultItems = new List<(Item Item, State State)>();

            while (currentState?.Items.Count > 0)
            {
                stats.NumberOfStates++;
                currentState.Checkpoint.Rewind();
                ParseCurrentState(parseState, initialState, states, currentState, resultItems, stats);
                currentState = states.MoveToNext();
            }

            var derivationVisitor = new ItemDerivationVisitor(stats);
            var results = resultItems
                .SelectMany(v => derivationVisitor.GetDerivation(v.Item)
                    .OfType<TOutput>()
                    .Select(value => new
                    {
                        Item = v.Item,
                        // The state.Number should be the number of input items consumed to get
                        // to that point, so we can use that here.
                        Consumed = v.State.Number,
                        Checkpoint = v.State.Checkpoint,
                        Value = value
                    })
                )
                .Select(x => new SuccessResultAlternative<TOutput>(x.Value, x.Consumed, x.Checkpoint))
                .ToList();

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

        private void ParseCurrentState(IParseState<TInput> parseState, State initialState, StateCollection states, State currentState, List<(Item Item, State State)> resultItems, ParseStatistics stats)
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
                    if (item.ParentState == initialState && _startSymbol.Productions.Contains(item.Production))
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

                // TODO: It's possible for .NextSymbolToMatch to be IMultiParser<TInput>, should
                // account for that here.
            }

            Debug.WriteLine(currentState.GetCompleteListing());
        }

        private static void Predict(State state, Item item, INonterminal nonterminal, IDictionary<IProduction, IList<Item>> completedNullables, ParseStatistics stats)
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
                foreach (var nullableItem in relevantCompletedNullableProductions.SelectMany(p => completedNullables[p]).Distinct().ToList())
                    aycockItem.Add(nullableItem);
                state.Add(aycockItem);
                stats.PredictedItems++;
                stats.PredictedByCompletedNullable++;
            }
        }

        private static void Scan(State currentState, Item item, IParser<TInput> terminal, StateCollection states, IParseState<TInput> input, ParseStatistics stats)
        {
            var result = terminal.Parse(input);
            if (!result.Success)
                return;

            var nextState = states.GetAhead(result.Consumed, input.Input);

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

        private static void AddCompletedNullable(IDictionary<IProduction, IList<Item>> completedNullables, Item item, IProduction production, ParseStatistics stats)
        {
            if (!completedNullables.ContainsKey(production))
                completedNullables[production] = new List<Item>();
            completedNullables[production].Add(item);
            stats.CompletedNullables++;
        }

        private static void Complete(State state, Item item, IDictionary<IProduction, IList<Item>> completedNullables, ParseStatistics stats)
        {
            // item is complete. Go through Items in the parent state which were waiting for item
            // and advance them.

            // For every item in the parent state which is incomplete and is waiting on a
            // non-terminal for which this production is an alternative, create a new Item in the
            // current state which is the same as the parent item, but advanced past the
            // non-terminal.
            var parentStateItemsToAdvance = item.ParentState.GetLiveItemsWaitingForProduction(item.Production);
            foreach (var parentItem in parentStateItemsToAdvance)
            {
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
                    var relevantCompletedNullables = nonterminal.Productions
                        .Where(p => completedNullables.ContainsKey(p))
                        .SelectMany(p => completedNullables[p])
                        .ToList();
                    foreach (var nullable in relevantCompletedNullables)
                        newItem.Add(nullable);
                }
            }

            // If the parent state is the current state, that means the item is zero-length and
            // is considered "nullable"
            if (item.ParentState == state)
                AddCompletedNullable(completedNullables, item, item.Production, stats);
        }
    }
}
