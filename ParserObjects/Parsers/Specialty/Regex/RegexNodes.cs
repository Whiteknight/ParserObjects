using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers.Specialty.Regex
{
    public interface IRegexNode
    {
        void BuildUpStates(List<List<RegexState>> states);
        string Description { get; }
    }

    public static class RegexNodes
    {
        public static IRegexNode Wildcard() => new MatchRegexNode(x => x != '\0', "Match any");
        public static IRegexNode Match(char c) => new MatchRegexNode(x => x == c, $"Match {c}");
        public static IRegexNode Match(Func<char, bool> predicate, string description) => new MatchRegexNode(predicate, description);

        public static IRegexNode ZeroOrOne(IRegexNode atom) => new ZeroOrOneQuantifiedRegexNode(atom);
        public static IRegexNode OneOrMore(IRegexNode atom) => new OneOrMoreQuantifiedRegexNode(atom);

        public static IRegexNode ZeroOrMore(IRegexNode atom) => new ZeroOrMoreQuantifiedRegexNode(atom);

        public static IRegexNode Range(IRegexNode atom, int min, int max) => new RangeQuantifiedRegexNode(atom, min, max);

        public static IRegexNode Group(IRegexNode node) => new GroupRegexNode(node);

        public static IRegexNode Sequence(IEnumerable<IRegexNode> nodes)
        {
            var nodeList = nodes.ToList();
            if (nodeList.Count == 1)
                return nodeList[0];
            return new SequenceRegexNode(nodeList);
        }

        public static IRegexNode Or(IEnumerable<IRegexNode> nodes)
        {
            var nodeList = nodes.ToList();
            if (nodeList.Count == 1)
                return nodeList[0];
            return new OrRegexNode(nodeList);
        }

        public static IRegexNode EndAnchor() => new EndAnchorRegexNode();

        public static IRegexNode CharacterClass(bool invert, IEnumerable<(char low, char high)> ranges)
        {
            var rangesList = ranges.ToList();
            return new CharacterClassRegexNode(invert, rangesList);
        }

        //public static IRegexNode Series(IEnumerable<IRegexNode> nodes)
        //{
        //}

        //public static IRegexNode FollowedBy(IRegexNode first, IRegexNode second)
        //{
        //}

        public static IRegexNode Nothing() => new NothingRegexNode();

        public class CharacterMatcher
        {
            private readonly HashSet<char> _exactChars;
            private readonly IReadOnlyList<(char low, char high)> _ranges;
            private readonly bool _invert;

            public CharacterMatcher(bool invert, IReadOnlyList<(char low, char high)> ranges)
            {
                _invert = invert;
                _exactChars = new HashSet<char>();
                var rangeList = new List<(char low, char high)>();
                foreach (var range in ranges)
                {
                    if (range.high == range.low)
                        _exactChars.Add(range.high);
                    else
                        rangeList.Add(range);
                }
                _ranges = rangeList;
            }

            public bool IsMatch(char c)
            {
                var isMatch = IsMatchBasic(c);
                return _invert ? !isMatch : isMatch;
            }

            private bool IsMatchBasic(char c)
            {
                if (_exactChars.Contains(c))
                    return true;
                foreach (var range in _ranges)
                {
                    if (range.low <= c && c <= range.high)
                        return true;
                }
                return false;
            }
        }

        private class CharacterClassRegexNode : IRegexNode
        {
            private readonly CharacterMatcher _matcher;

            public CharacterClassRegexNode(bool invert, IReadOnlyList<(char low, char high)> ranges)
            {
                _matcher = new CharacterMatcher(invert, ranges);
            }

            public string Description => "range";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                states.Last().Add(new RegexState
                {
                    Type = RegexStateType.MatchValue,
                    ValuePredicate = c => _matcher.IsMatch(c),
                    Quantifier = RegexQuantifier.ExactlyOne,
                    Description = Description
                });
            }
        }

        private class NothingRegexNode : IRegexNode
        {
            public string Description => "nothing";

            public void BuildUpStates(List<List<RegexState>> states)
            {
            }
        }

        private class OrRegexNode : IRegexNode
        {
            private readonly IReadOnlyList<IRegexNode> _nodes;

            public OrRegexNode(IReadOnlyList<IRegexNode> nodes)
            {
                _nodes = nodes;
            }

            public string Description => "alternation";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                var theseStates = new List<List<RegexState>>();
                foreach (var node in _nodes)
                {
                    theseStates.Add(new List<RegexState>());
                    node.BuildUpStates(theseStates);
                }

                states.Last().Add(new RegexState
                {
                    Type = RegexStateType.Alternation,
                    Alternations = theseStates,
                    Description = Description
                });
            }
        }

        private class EndAnchorRegexNode : IRegexNode
        {
            public string Description => "end";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                states.Last().Add(new RegexState
                {
                    Type = RegexStateType.EndOfInput,
                    Description = Description
                });
            }
        }

        private class SequenceRegexNode : IRegexNode
        {
            private readonly IReadOnlyList<IRegexNode> _nodes;

            public SequenceRegexNode(IReadOnlyList<IRegexNode> nodes)
            {
                _nodes = nodes;
            }

            public string Description => "";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                foreach (var node in _nodes)
                    node.BuildUpStates(states);
            }
        }

        private class GroupRegexNode : IRegexNode
        {
            private readonly IRegexNode _node;

            public GroupRegexNode(IRegexNode node)
            {
                _node = node;
            }

            public string Description => "group";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                states.Add(new List<RegexState>());
                _node.BuildUpStates(states);
                var groupStates = states.Last();
                states.RemoveAt(states.Count - 1);
                if (groupStates.Count == 1)
                {
                    states.Last().Add(groupStates[0]);
                    return;
                }
                states.Last().Add(new RegexState
                {
                    Type = RegexStateType.Group,
                    Group = groupStates,
                    Quantifier = RegexQuantifier.ExactlyOne,
                    Description = Description
                });
            }
        }

        private class MatchRegexNode : IRegexNode
        {
            private readonly Func<char, bool> _predicate;

            public MatchRegexNode(Func<char, bool> predicate, string description)
            {
                _predicate = predicate;
                Description = description;
            }

            public string Description { get; }

            public void BuildUpStates(List<List<RegexState>> states)
            {
                states.Last().Add(new RegexState
                {
                    Type = RegexStateType.MatchValue,
                    ValuePredicate = _predicate,
                    Quantifier = RegexQuantifier.ExactlyOne,
                    Description = Description
                });
            }

            public override string ToString() => Description;
        }

        private class ZeroOrOneQuantifiedRegexNode : IRegexNode
        {
            private readonly IRegexNode _atom;

            public ZeroOrOneQuantifiedRegexNode(IRegexNode atom)
            {
                _atom = atom;
            }

            public string Description => "";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                _atom.BuildUpStates(states);
                var lastElement = states.Last().LastOrDefault();
                if (lastElement == null || lastElement.Quantifier != RegexQuantifier.ExactlyOne)
                {
                    // TODO: Need to communicate an error condition here but Exception is probably not what we want
                    throw new Exception("Quantifier '?' may only follow an unquantified atom");
                }
                lastElement.Quantifier = RegexQuantifier.ZeroOrOne;
            }
        }

        private class ZeroOrMoreQuantifiedRegexNode : IRegexNode
        {
            private readonly IRegexNode _atom;

            public ZeroOrMoreQuantifiedRegexNode(IRegexNode atom)
            {
                _atom = atom;
            }

            public string Description => "";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                _atom.BuildUpStates(states);
                var lastElement = states.Last().Last();
                if (lastElement == null || lastElement.Quantifier != RegexQuantifier.ExactlyOne)
                {
                    // TODO: Need to communicate an error condition here but Exception is probably not what we want
                    throw new Exception("Quantifier '*' may only follow an unquantified atom");
                }
                lastElement.Quantifier = RegexQuantifier.ZeroOrMore;
            }
        }

        private class OneOrMoreQuantifiedRegexNode : IRegexNode
        {
            private readonly IRegexNode _atom;

            public OneOrMoreQuantifiedRegexNode(IRegexNode atom)
            {
                _atom = atom;
            }

            public string Description => "";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                // Build up the atom twice. First will will have a quantifier of ExactlyOne. Second
                // one we will modify to be ZeroOrMore
                _atom.BuildUpStates(states);
                _atom.BuildUpStates(states);
                var lastElement = states.Last().Last();
                if (lastElement == null || lastElement.Quantifier != RegexQuantifier.ExactlyOne)
                {
                    // TODO: Need to communicate an error condition here but Exception is probably not what we want
                    throw new Exception("Quantifier '+' may only follow an unquantified atom");
                }
                lastElement.Quantifier = RegexQuantifier.ZeroOrMore;
            }
        }

        private class RangeQuantifiedRegexNode : IRegexNode
        {
            private readonly IRegexNode _atom;
            private readonly int _min;
            private readonly int _max;

            public RangeQuantifiedRegexNode(IRegexNode atom, int min, int max)
            {
                _atom = atom;
                if (_min > _max)
                    throw new Exception($"Invalid range. Minimum {min} must be smaller or equal to maximum {max}");
                if (min < 0 || max == 0 || max < -1)
                    throw new Exception("Invalid range. Minimum must be 0 or more, and maximum must be 1 or more");
                _min = min;
                _max = max;
            }

            public string Description => $"{{{_min}, {_max}}}";

            public void BuildUpStates(List<List<RegexState>> states)
            {
                if (_min > 0)
                {
                    // Copy this state Minimum times, since we must have them all
                    for (int i = 0; i < _min; i++)
                        _atom.BuildUpStates(states);
                }

                if (_min == _max)
                    return;

                if (_max == -1)
                {
                    // No maximum, so we can treat the remainder like ZeroOrMore
                    _atom.BuildUpStates(states);
                    var lastElement = states.Last().Last();
                    if (lastElement == null || lastElement.Quantifier != RegexQuantifier.ExactlyOne)
                    {
                        // TODO: Need to communicate an error condition here but Exception is probably not what we want
                        throw new Exception("Range maximum may only follow an unquantified atom");
                    }
                    lastElement.Quantifier = RegexQuantifier.ZeroOrMore;
                    return;
                }

                var realMax = _max - _min;
                if (realMax >= 1)
                {
                    // We have a maximum value, so add a new state and set it's quantifier to range
                    _atom.BuildUpStates(states);
                    var lastElement = states.Last().Last();
                    if (lastElement == null || lastElement.Quantifier != RegexQuantifier.ExactlyOne)
                    {
                        // TODO: Need to communicate an error condition here but Exception is probably not what we want
                        throw new Exception("Range maximum may only follow an unquantified atom");
                    }
                    lastElement.Quantifier = RegexQuantifier.Range;
                    lastElement.Maximum = _max - _min;
                }
            }
        }
    }
}
