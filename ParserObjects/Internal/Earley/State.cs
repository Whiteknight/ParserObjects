using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

// An "Earley State", which holds all in-progress Items at the current position of input.
public sealed class State
{
    public State(int number, SequenceCheckpoint cp)
    {
        Items = [];
        Number = number;
        Checkpoint = cp;
    }

    public IList<Item> Items { get; }

    // The number of the state, which corresponds to the number of input items consumed at
    // this point.
    public int Number { get; }

    // The checkpoint to return the input to the position of this state
    public SequenceCheckpoint Checkpoint { get; }

    public void Add(Item item)
    {
        if (!Contains(item))
            Items.Add(item);
    }

    public bool Contains(Item item) => Items.Contains(item);

    public RentedArray<Item> GetLiveItemsWaitingForProduction(IProduction production)
    {
        var results = ArrayPool<Item>.Shared.Rent(Items.Count);
        int count = 0;
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.IsWaitingFor(production))
                results[count++] = item;
        }

        return new RentedArray<Item>(results, count);
    }

    public Item Import(Item item)
    {
        var existing = Items.FirstOrDefault(i => i.Equals(item));
        if (existing == null)
        {
            Add(item);
            return item;
        }

        Debug.Assert(existing.CanImport(item));
        return existing;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"State {Number}";

    [ExcludeFromCodeCoverage]
    public string GetCompleteListing()
    {
        var sb = new StringBuilder();
        sb.Append("==== S").Append(Number).AppendLine(" ====");
        foreach (var item in Items)
            sb.Append(item.ToString());
        return sb.ToString();
    }
}
