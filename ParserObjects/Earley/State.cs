using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ParserObjects.Earley;

// An "Earley State", which holds all in-progress Items at the current position of input.
public class State
{
    public State(int number, ISequenceCheckpoint cp)
    {
        Items = new List<Item>();
        Number = number;
        Checkpoint = cp;
    }

    public IList<Item> Items { get; }

    // The number of the state, which corresponds to the number of input items consumed at
    // this point.
    public int Number { get; }

    // The checkpoint to return the input to the position of this state
    public ISequenceCheckpoint Checkpoint { get; }

    public void Add(Item item)
    {
        if (!Contains(item))
            Items.Add(item);
    }

    public bool Contains(Item item) => Items.Contains(item);

    public IList<Item> GetLiveItemsWaitingForProduction(IProduction production)
        => Items
            .Where(i => i.IsWaitingFor(production))
            .ToList();

    public Item Import(Item item)
    {
        var existing = Items.FirstOrDefault(i => i.Equals(item));
        if (existing == null)
        {
            Add(item);
            return item;
        }

        Debug.Assert(existing.CanImport(item), "Imported item must be Equal and not have any derivations");
        return existing;
    }

    public override string ToString() => $"State {Number}";

    public string GetCompleteListing()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"==== S{Number} ====");
        foreach (var item in Items)
            sb.Append(item.ToString());
        return sb.ToString();
    }
}
