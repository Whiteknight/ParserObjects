using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParserObjects.Earley;

public class ItemDerivationVisitor
{
    private readonly Dictionary<(IProduction, int, int), IReadOnlyList<object>> _cache;
    private readonly ParseStatistics _statistics;

    public ItemDerivationVisitor(ParseStatistics statistics)
    {
        _cache = new Dictionary<(IProduction, int, int), IReadOnlyList<object>>();
        _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
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
        {
            _statistics.DerivationCacheHit++;
            return _cache[key];
        }

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
            _statistics.DerivationSingleSymbolShortcircuits++;
            return endItem.Derivations!
                .Select(d => production.Apply(new[] { d }))
                .Where(o => o.Success)
                .Select(o => o.Value)
                .ToList();
        }

        Item current = endItem;
        var count = production.Symbols.Count;
        var values = ArrayPool<IReadOnlyList<object>>.Shared.Rent(count);
        if (!GetValuesArray(values, current))
            return Array.Empty<object>();

        // Small shortcircuit: If there is only one item in the production, and that one item
        // has only one possible value after recursing, just return that without buffering.
        if (count == 1 && values[0].Count == 1)
        {
            var result = production.Apply(new[] { values[0][0] });
            _statistics.ItemsWithSingleDerivation++;
            return result.Success ? new object[] { result.Value } : Array.Empty<object>();
        }

        // Allocate a buffer and fill with initial values
        var buffer = ArrayPool<object>.Shared.Rent(count);
        InitializeBuffer(count, values, buffer);

        // Get an array to hold indices and initialize all values (they might not be 0
        // coming out of the array pool)
        var indices = ArrayPool<int>.Shared.Rent(count);
        Array.Clear(indices, 0, count);

        // We traverse from Item to Item.ParentItem, which forms a unique chain from the end
        // back to the beginning. Even if Item.ParentItem has multiple children of different
        // lengths we can traverse those branches separately and there is no ambiguity in
        // derivation of results.

        // Start getting values. On each loop we call the production callback with what we
        // have in the buffer already, then we update values to the next index. When position
        // 1 gets to the last value, we reset it to 0 and increment position 2, etc. When
        // we've incremented the last position past the number of items in the last column, we
        // break from the loop and are done.
        var results = new List<object>();
        do
        {
            _statistics.ProductionRuleAttempts++;
            var result = production.Apply(buffer);
            if (result.Success)
            {
                results.Add(result.Value);
                _statistics.ProductionRuleSuccesses++;
            }
        }
        while (IncrementBufferItems(count, indices, values, buffer));

        ArrayPool<IReadOnlyList<object>>.Shared.Return(values);
        ArrayPool<object>.Shared.Return(buffer);
        ArrayPool<int>.Shared.Return(indices);
        return results;
    }

    private static void InitializeBuffer(int count, IReadOnlyList<object>[] values, object[] buffer)
    {
        for (int i = 0; i < count; i++)
            buffer[i] = values[i][0];
    }

    private bool GetValuesArray(IReadOnlyList<object>[] values, Item current)
    {
        // Traverse the linked list of items, getting all possibilities for each item.
        // If any item has zero options, there's no way to produce a derivation result so we
        // bail.
        while (!current.AtStart)
        {
            var possibleValuesForThisItem = GetAllPossibleValuesFor(current);
            if (possibleValuesForThisItem.Count == 0)
            {
                _statistics.ItemsWithZeroDerivations++;
                return false;
            }

            values[current.Index - 1] = possibleValuesForThisItem;
            current = current.Previous!;
        }

        return true;
    }

    // Returns a list of all possible values for this Item.
    private IReadOnlyList<object> GetAllPossibleValuesFor(Item item)
    {
        if (item.AtStart)
            throw new InvalidOperationException();

        // A Terminal has a single value, so _derivations.Count should be 1 and it should be
        // a value, not another item. So we can just return that list.
        if (item.ValueSymbol is IParser)
            return item.Derivations!;

        // A nonterminal potentially contains a number of productions, which are all
        // alternations. For each alternation option, we want to reduce to the appropriate
        // value and add that to the list of values.
        if (item.ValueSymbol is INonterminal)
            return item.Derivations!.OfType<Item>().SelectMany(d => Visit(d)).ToList();

        return Array.Empty<object>();
    }

    private static bool IncrementBufferItems(int count, int[] indices, IReadOnlyList<object>[] values, object[] buffer)
    {
        Debug.Assert(indices.Length >= count, "The indices array should have at least as many items as the count");
        Debug.Assert(values.Length >= count, "The values array should have at least as many items as the count");
        Debug.Assert(buffer.Length >= count, "The buffer should have at least as many slots as the count");

        // Incrementing happens after we produce a value. So the first iteration we have already
        // produced (0, 0, ..., 0) so we start here immediately by incrementing to (1, 0, ...)
        // Then we handle rollover logic and update the buffer if we aren't at the end.

        for (var idx = 0; ; idx++)
        {
            indices[idx]++;

            // If the current column doesn't rollover, update just one buffer entry
            // and break
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
