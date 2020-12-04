using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParserObjects.Earley
{
    // Engine for the Earley parser algorithm. Manages states and process, and returns the final
    // result
    public class Engine<TInput, TOutput>
    {
        private readonly INonterminal<TInput, TOutput> _startSymbol;

        public Engine(INonterminal<TInput, TOutput> startSymbol)
        {
            if (startSymbol.Productions.Count == 0 || startSymbol.Productions.All(p => p.Symbols.Count == 0))
                throw new ArgumentException();

            _startSymbol = startSymbol;
        }

        public IReadOnlyList<IMultiResultAlternative<TOutput>> Parse(IParseState<TInput> state)
        {
            var startCheckpoint = state.Input.Checkpoint();
            var states = new StateCollection(startCheckpoint);

            var initialState = states.InitialState;
            foreach (var production in _startSymbol.Productions)
                initialState.Add(new Item(production, initialState, initialState));
            var currentState = initialState;

            var resultItems = new List<(Item Item, State State)>();

            // TODO: Leo Optimization for right-recursive rules (and a way to test it, the only
            // way we'll know it's working is if we generate fewer internal states, which aren't
            // exposed outside the engine).

            while (true)
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
                        Complete(currentState, item, completedNullables);
                        if (item.ParentState == initialState && _startSymbol.Productions.Contains(item.Production))
                            resultItems.Add((item, currentState));
                        continue;
                    }

                    // If the item is a non-terminal, predict what the state should look like and
                    // add those predicted items to the current state.
                    if (item.NextSymbolToMatch is INonterminal nonterminal)
                    {
                        Predict(currentState, item, nonterminal, completedNullables);
                        continue;
                    }

                    // If the item is terminal, see if we match, and add successful matches to the
                    // next state.
                    if (item.NextSymbolToMatch is IParser<TInput> terminal)
                    {
                        Scan(currentState, item, terminal, states, state);
                        continue;
                    }
                }

                Debug.WriteLine(currentState.GetCompleteListing());

                var nextState = states.MoveToNext();
                if (nextState == null || nextState.Items.Count == 0)
                    break;

                currentState = nextState;
                currentState.Checkpoint.Rewind();
            }

            var derivationVisitor = new ItemDerivationVisitor();
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
                .Select(x => new SuccessMultiResultAlternative<TOutput>(x.Value, x.Consumed, x.Checkpoint))
                .ToList();

            return results;
        }

        private void Predict(State state, Item item, INonterminal nonterminal, IDictionary<IProduction, IList<Item>> completedNullables)
        {
            // Add prediction states
            var relevantCompletedNullableProductions = new List<IProduction>();
            foreach (var p in nonterminal.Productions)
            {
                var newItem = new Item(p, state, state);
                if (completedNullables.ContainsKey(p))
                    relevantCompletedNullableProductions.Add(p);
                if (state.Contains(newItem))
                    continue;
                state.Add(newItem);
            }

            // Aycock fix: If this item is waiting on a symbol which is already in the list of
            // nullables, we can simply advance it here
            if (relevantCompletedNullableProductions.Any())
            {
                var aycockItem = state.Import(item.CreateNextItem(state));
                foreach (var nullableItem in relevantCompletedNullableProductions.SelectMany(p => completedNullables[p]).Distinct().ToList())
                    aycockItem.Add(nullableItem);
                state.Add(aycockItem);
            }
        }

        private void Scan(State currentState, Item item, IParser<TInput> terminal, StateCollection states, IParseState<TInput> input)
        {
            var result = terminal.Parse(input);
            if (!result.Success)
                return;

            var nextState = states.GetAhead(result.Consumed, input.Input);

            var newItem = item.CreateNextItem(nextState);

            // Add the value to the new item's _derivations list. The value of a Terminal is the
            // returned value from the parser.
            newItem.SetTerminalValue(result.Value);
            nextState.Add(newItem);

            if (result.Consumed > 0)
                currentState.Checkpoint.Rewind();
        }

        private void AddCompletedNullable(IDictionary<IProduction, IList<Item>> completedNullables, Item item, IProduction production)
        {
            if (!completedNullables.ContainsKey(production))
                completedNullables[production] = new List<Item>();
            completedNullables[production].Add(item);
        }

        private void Complete(State state, Item item, IDictionary<IProduction, IList<Item>> completedNullables)
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

                // Add the item to the newItem's _derivations list. When we derive the value, we
                // need to recurse from newItem->item to get that part of it.
                newItem.Add(item);
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

            // If Item.ParentState == state, that means this item is nullable.
            if (item.ParentState == state)
                AddCompletedNullable(completedNullables, item, item.Production);
        }
    }
}
