using System;
using System.Collections.Generic;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Earley
{
    // Nonterminal contains a list of productions which represent alternative possibilities. In
    // this sense, Nonterminal is the Earley-world equivalent of First()
    // A Production is an ordered list of symbols which must be matched in order and contain a
    // derivation callback. Production is the Earley-world equivalent of Rule()
    public class Nonterminal<TInput, TOutput> : INonterminal<TInput, TOutput>
    {
        private readonly HashSet<Production<TOutput>> _productions;

        public Nonterminal(string name)
        {
            _productions = new HashSet<Production<TOutput>>();
            Name = string.IsNullOrEmpty(name) ? $"N{UniqueIntegerGenerator.GetNext()}" : name;
        }

        public IReadOnlyCollection<IProduction> Productions => _productions;

        public void AddProductionObj(IProduction p)
        {
            if (p is not Production<TOutput> typed)
                throw new InvalidOperationException("Production must have a matching output type");
            if (!_productions.Contains(typed))
                _productions.Add(typed);
        }

        public bool Contains(IProduction p)
            => p is Production<TOutput> typed && _productions.Contains(typed);

        public string Name { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var p in _productions)
                sb.Append(p);

            return sb.ToString();
        }
    }
}
