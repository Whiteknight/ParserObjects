using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public sealed class MatchCharacterClassState : IState
{
    // Printable ASCII chars go from 0x32 (' ') to 0x7E ('~'). _exactChars array is an optimization
    // where each character in that range corresponds to a bit in a 12-byte array.
    private readonly byte[] _exactChars;

    private readonly IReadOnlyList<(char low, char high)> _ranges;
    private readonly bool _invert;

    public MatchCharacterClassState(string name, bool invert, IReadOnlyList<(char low, char high)> ranges)
    {
        Name = name;
        _invert = invert;
        var chars = new byte[12];

        var rangeList = new List<(char low, char high)>();
        for (int i = 0; i < ranges.Count; i++)
        {
            var (low, high) = ranges[i];
            if (low >= ' ' && high <= '~')
            {
                for (char c = low; c <= high; c++)
                    SetCharBit(chars, c);
                continue;
            }

            rangeList.Add((low, high));
        }

        _exactChars = chars;
        _ranges = rangeList;
    }

    private MatchCharacterClassState(string name, bool invert, byte[] exactChars, IReadOnlyList<(char, char)> ranges)
    {
        Name = name;
        _invert = invert;
        _exactChars = exactChars;
        _ranges = ranges;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public string Name { get; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => new MatchCharacterClassState(name, _invert, _exactChars, _ranges)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public bool Match(RegexContext context, SequenceCheckpoint checkpoint, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        if (!IsMatch(context.Input.Peek()))
            return false;

        context.Input.GetNext();
        return true;
    }

    private bool IsMatch(char c)
    {
        var isMatch = IsMatchBasic(c);
        // small strength reduction from "_invert ? !isMatch : isMatch"
        return _invert ^ isMatch;
    }

    private static void SetCharBit(byte[] chars, char c)
    {
        var intVal = c - ' ';
        var index = intVal / 8;
        var position = intVal % 8;
        var flag = (byte)(1 << position);
        chars[index] |= flag;
    }

    private bool IsMatchBasic(char c)
    {
        if (c >= ' ' && c <= '~')
        {
            var intVal = c - ' ';
            var index = intVal / 8;
            var position = intVal % 8;
            var flag = (byte)(1 << position);
            return (_exactChars[index] & flag) > 0;
        }

        // TODO: Need unit tests for these cases. Basically, tests for non-printable ASCII chars
        for (int i = 0; i < _ranges.Count; i++)
        {
            if (_ranges[i].low <= c && c <= _ranges[i].high)
                return true;
        }

        return false;
    }
}
