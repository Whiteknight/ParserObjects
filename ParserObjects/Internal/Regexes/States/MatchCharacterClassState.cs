using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

public sealed class MatchCharacterClassState : IState
{
    // Printable ASCII chars go from 0x32 (' ') to 0x7E ('~'). _exactChars array is an optimization
    // where each character in that range corresponds to a bit in a 12-byte array.
    private readonly byte[]? _exactChars;

    private readonly IReadOnlyList<(char Low, char High)>? _ranges;
    private readonly bool _invert;

    public MatchCharacterClassState(bool invert, byte[]? exactChars, IReadOnlyList<(char, char)>? ranges)
    {
        _invert = invert;
        _exactChars = exactChars;
        _ranges = ranges;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} class";

    public IState Clone() => new MatchCharacterClassState(_invert, _exactChars, _ranges)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        if (!IsMatch(context.Input.Peek()))
            return false;

        context.Input.GetNext();
        return true;
    }

    private bool IsMatch(char c)
        => _invert ^ IsMatchBasic(c);

    private bool IsMatchBasic(char c)
    {
        if (c >= ' ' && c <= '~' && _exactChars != null)
        {
            var intVal = c - ' ';
            return (_exactChars[intVal / 8] & (byte)(1 << intVal % 8)) > 0;
        }

        if (_ranges != null)
        {
            for (int i = 0; i < _ranges.Count; i++)
            {
                if (_ranges[i].Low <= c && c <= _ranges[i].High)
                    return true;
            }
        }

        return false;
    }
}
