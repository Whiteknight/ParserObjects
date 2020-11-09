using System;
using System.Collections.Generic;

namespace ParserObjects.Regexes
{
    public class RegexState
    {
        public RegexStateType Type { get; set; }
        public bool CanBacktrack { get; set; }
        public RegexQuantifier Quantifier { get; set; }
        public Func<char, bool> ValuePredicate { get; set; }
        public List<RegexState> Group { get; set; }

        public List<List<RegexState>> Alternations { get; set; }
        public string Description { get; set; }
        public int Maximum { get; set; }

        public override string ToString()
        {
            return $"{Quantifier} {Description}";
        }
    }
}
