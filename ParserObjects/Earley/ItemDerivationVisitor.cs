using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParserObjects.Earley
{
    // TODO: This needs to be optimized. See if the Scott SPPF algorithm can be made to work here.
    public class ItemDerivationVisitor
    {
        private readonly Dictionary<(IProduction, int, int), IReadOnlyList<object>> _cache;

        public ItemDerivationVisitor()
        {
            _cache = new Dictionary<(IProduction, int, int), IReadOnlyList<object>>();
        }

        // Items work in a weird way. The _derivations list contains all possible values for the
        // symbol directly to the LEFT of the fat dot, which means they correspond to
        // item.ValueSymbol. This is why item.AtStart can never have a value, because
        // there is nothing to the left of the fat dot.

        public IReadOnlyList<object> GetDerivation(Item item) => Visit(item);

        private IReadOnlyList<object> Visit(Item endItem)
        {
            // For every Item which is the last item of a production, we want to traverse the
            // linked-list of item->item.Previous to get items for all symbols in the production.
            // We get all possible values from each item, and then invoke the production callback
            // for every combination of items in each position. For unambiguous grammars this is
            // O(n). For highly-ambiguous grammars it goes off the rails pretty quick.

            var production = endItem.Production;

            Debug.Assert(endItem.Index == production.Symbols.Count, "This is the end item of this production");

            var key = (production, endItem.ParentState.Number, endItem.CurrentState.Number);
            if (_cache.ContainsKey(key))
                return _cache[key];

            var results = GenerateValues(endItem, production);

            _cache.Add(key, results);

            return results;
        }

        private IReadOnlyList<object> GenerateValues(Item endItem, IProduction production)
        {
            // Small shortcircuit: If this production has one symbol, and that symbol is a
            // terminal, just return those derivation results directly without any buffering.
            if (production.Symbols.Count == 1 && production.Symbols[0] is IParser)
            {
                return endItem.Derivations!
                    .Select(d => production.Apply(new[] { d }))
                    .Where(o => o.Success)
                    .Select(o => o.Value)
                    .ToList();
            }

            Item current = endItem;
            var count = production.Symbols.Count;
            var values = new IReadOnlyList<object>[count];

            // Traverse the linked list of items, getting all possibilities for each item.
            // If any item has zero options, there's no way to produce a derivation result so we
            // bail.
            while (!current.AtStart)
            {
                var possibleValuesForThisItem = GetAllPossibleValuesFor(current);
                if (possibleValuesForThisItem.Count == 0)
                    return new List<object>();
                values[current.Index - 1] = possibleValuesForThisItem;
                current = current.Previous!;
            }

            // Small shortcircuit: If there is only one item in the production, and that one item
            // has only one possible value after recursing, just return that without buffering.
            if (count == 1 && values[0].Count == 1)
            {
                var result = production.Apply(new[] { values[0][0] });
                return result.Success ? new List<object> { result.Value } : Array.Empty<object>();
            }

            var buffer = new object[count];

            // Fill the buffer with initial values
            for (int i = 0; i < count; i++)
                buffer[i] = values[i][0];

            // Start getting values. On each loop we call the production callback with what we
            // have in the buffer already, then we update values to the next index. When position
            // 1 gets to the last value, we reset it to 0 and increment position 2, etc. When
            // we've incremented the last position past the number of items in the last column, we
            // break from the loop and are done.
            var indices = new int[count];
            var results = new List<object>();
            while (true)
            {
                var result = production.Apply(buffer);
                if (result.Success)
                    results.Add(result.Value);
                var hasMore = IncrementBufferItems(count, indices, values, buffer);
                if (!hasMore)
                    break;
            }

            return results;
        }

        // terminal.Derivations is a list of result values, so return those directly.
        private IReadOnlyList<object> VisitTerminalItem(Item item)
            => item.Derivations!;

        // nonterminal.Derivations is a list of Items, which we can recurse into to get their
        // values.
        private IReadOnlyList<object> VisitNonterminal(Item item)
            => item.Derivations!.OfType<Item>().SelectMany(d => Visit(d)).ToList();

        // Returns a list of all possible values for this Item.
        private IReadOnlyList<object> GetAllPossibleValuesFor(Item item)
        {
            if (item.AtStart)
                throw new InvalidOperationException();

            // A Terminal has a single value, so _derivations.Count should be 1 and it should be
            // a value, not another item. So we can just return that list.
            if (item.ValueSymbol is IParser)
                return VisitTerminalItem(item);

            // A nonterminal potentially contains a number of productions, which are all
            // alternations. For each alternation option, we want to reduce to the appropriate
            // value and add that to the list of values.
            if (item.ValueSymbol is INonterminal)
                return VisitNonterminal(item);

            return new List<object>();
        }

        private bool IncrementBufferItems(int count, int[] indices, IReadOnlyList<object>[] values, object[] buffer)
        {
            for (var idx = 0; ; idx++)
            {
                indices[idx]++;

                // If the current column doesn't rollover, update just one buffer entry
                // and break;
                if (indices[idx] < values[idx].Count)
                {
                    buffer[idx] = values[idx][indices[idx]];
                    return true;
                }

                // If we're at the last column and would rollover, we're done. Need to
                // break out of the loop.
                if (idx == count - 1)
                    return false;

                // Rollover this column, update the buffer item to idx=0, and allow the loop
                // to continue so we increment the next column
                indices[idx] = 0;
                buffer[idx] = values[idx][0];
            }
        }
    }
}
