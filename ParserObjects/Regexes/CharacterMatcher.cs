﻿using System.Collections.Generic;

namespace ParserObjects.Regexes
{
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
            foreach (var (low, high) in _ranges)
            {
                if (low <= c && c <= high)
                    return true;
            }

            return false;
        }
    }
}
