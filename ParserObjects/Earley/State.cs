using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserObjects.Earley
{
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

        public int Number { get; }

        public ISequenceCheckpoint Checkpoint { get; }

        public void Add(Item item)
        {
            if (!Contains(item))
                Items.Add(item);
        }

        public bool Contains(Item item) => Items.Contains(item);

        // TODO: Rename this to something more descriptive
        public IList<Item> GetItems(IProduction production)
        {
            return Items
                .Where(i => !i.AtEnd && i.NextSymbolToMatch is INonterminal nonterminal && nonterminal.Productions.Contains(production))
                .ToList();
        }

        public Item Import(Item item)
        {
            var existing = Items.FirstOrDefault(i => i.Equals(item));
            if (existing == null)
            {
                Add(item);
                return item;
            }

            if (!existing.CanImport(item))
                throw new ArgumentException();
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
}
