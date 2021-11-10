using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Earley
{
    // An "Earley Item", which is a production rule with an index representing the position of the
    // "fat dot". The fat dot separates the part of the production which has been matched (left)
    // from the part of the production which has not yet been matched (right).
    public sealed class Item : IEquatable<Item>
    {
        private readonly List<object>? _derivations;

        public Item(IProduction production, State parent, State currentState)
            : this(production, 0, parent, currentState, null)
        {
        }

        public Item(IProduction production, int index, State parent, State currentState, Item? previous)
        {
            Assert.ArgumentNotNull(production, nameof(production));
            Assert.ArgumentNotNull(parent, nameof(parent));

            Production = production;
            Index = index;

            Previous = previous;
            _derivations = index == 0 ? null : new List<object>();

            // ParentState is the state where this item started. CurrentState is the currently
            // active state where this item currenly resides. We need references here instead of
            // just the integer state numbers, because old States won't be referenced anymore
            // and they will get GC'd if we don't keep reference to them if we need them.
            ParentState = parent;
            CurrentState = currentState;
        }

        public bool AtStart => Index == 0;
        public bool AtEnd => Index == Production.Symbols.Count;
        public State ParentState { get; }
        public IProduction Production { get; }
        public int Index { get; }

        // Items

        // Matched values from the symbol directly to the LEFT of the fat dot
        public IReadOnlyList<object>? Derivations => _derivations;

        public Item? Previous { get; }

        // The symbol directly to the RIGHT of the fat dot. The next symbol to match.
        public ISymbol NextSymbolToMatch
        {
            get
            {
                Debug.Assert(!AtEnd, "End items do not have next symbols to match");
                return Production.Symbols[Index];
            }
        }

        // The symbol directly to the LEFT of the fat dot. The most recently matched symbol
        public ISymbol ValueSymbol
        {
            get
            {
                Debug.Assert(!AtStart, "Start items do not have values");
                return Production.Symbols[Index - 1];
            }
        }

        public State CurrentState { get; }

        public override bool Equals(object? obj) => obj is Item other && Equals(other);

        public bool Equals(Item? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return ReferenceEquals(Production, other.Production)
                && Index == other.Index
                && ReferenceEquals(ParentState, other.ParentState)
                && ReferenceEquals(CurrentState, other.CurrentState)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ParentState.Number ^ Production.GetHashCode() ^ Index ^ CurrentState.Number;
            }
        }

        // If this Item has a production with a single item, and that item is a terminal, set the
        // single value
        public void SetTerminalValue(object input)
        {
            Debug.Assert(!AtStart, "Start items cannot have values");
            Debug.Assert(ValueSymbol is IParser, "Only parser (terminal) symbols may have values");
            _derivations!.Add(input);
        }

        public void Add(Item item)
        {
            Debug.Assert(!AtStart, "Start items cannot have values");
            var previousNonterminal = ValueSymbol as INonterminal;
            Debug.Assert(previousNonterminal != null, "Only non-terminal symbols may point to Items");
            Debug.Assert(previousNonterminal.Contains(item.Production), "The item should have the same production");

            if (_derivations!.Contains(item))
                return;
            _derivations!.Add(item);
        }

        public Item CreateNextItem(State currentState)
        {
            Debug.Assert(!AtEnd, "End items cannot be advanced");
            return new Item(Production, Index + 1, ParentState, currentState, this);
        }

        public bool CanImport(Item other)
        {
            return Equals(other)
                && ((other._derivations == null && _derivations == null) ||
                    (other._derivations != null && other._derivations.Count == 0));
        }

        public bool IsWaitingFor(IProduction production)
            => !AtEnd && NextSymbolToMatch is INonterminal nonterminal && nonterminal.Productions.Contains(production);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Production.LeftHandSide.Name);
            sb.Append(" := ");
            for (int i = 0; i < Production.Symbols.Count; i++)
            {
                if (i == Index)
                    sb.Append("* ");
                sb.Append($"<{Production.Symbols[i].Name}> ");
            }

            if (Index == Production.Symbols.Count)
                sb.Append("* ");

            sb.AppendLine($" ({ParentState.Number},{CurrentState.Number})");
            return sb.ToString();
        }
    }
}
