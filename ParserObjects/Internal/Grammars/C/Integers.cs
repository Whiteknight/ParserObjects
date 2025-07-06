using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.C;

public static class Integers
{
    private const string _errorCannotStartWithZero = "Cannot start with a zero";

    private static readonly IParser<char, char> _octalDigit = Match(c => char.IsDigit(c) && c >= '0' && c <= '7');

    public static IParser<char, int> CreateHexSignedIntegerParser()
        => Sequential(static s => (int)ParseSignedHexLong(s, 8))
            .Named("C-Style Hex Signed Integer Literal");

    public static IParser<char, uint> CreateHexUnsignedIntegerParser()
        => Sequential(static s => (uint)ParseUnsignedHexLong(s, 8));

    public static IParser<char, long> CreateHexSignedLongParser()
        => Sequential(static s => ParseSignedHexLong(s, 16))
            .Named("C-Style Hex Signed Long Literal");

    public static IParser<char, ulong> CreateHexUnsignedLongParser()
        => Sequential(static s => ParseUnsignedHexLong(s, 16))
            .Named("C-Style Hex Unsigned Long Literal");

    public static IParser<char, int> CreateSignedIntegerParser()
    {
        var decimalInteger = Sequential(static s =>
        {
            var sign = s.TryParse(MatchChar('-')).Success;

            // Get the first digit, which cannot be zero.
            var d = s.Parse(Digit());
            if (d == '0')
                s.Fail(_errorCannotStartWithZero);
            return ParseSignedIntegerBodyWithBase(s, 10, Digit(), d - '0', sign);
        });

        var octalInteger = Sequential(static s =>
        {
            var sign = s.TryParse(MatchChar('-')).Success;
            s.Expect(MatchChar('0'));
            return ParseSignedIntegerBodyWithBase(s, 8, _octalDigit, 0, sign);
        });

        return First(
            decimalInteger,
            CreateHexSignedIntegerParser(),
            octalInteger
        );
    }

    public static IParser<char, uint> CreateUnsignedIntegerParser()
    {
        var decimalInteger = Sequential(static s =>
        {
            // Get the first digit, which cannot be zero.
            var d = s.Parse(Digit());
            if (d == '0')
                s.Fail(_errorCannotStartWithZero);
            return ParseUnsignedIntegerBodyWithBase(s, 10, Digit(), (uint)(d - '0'));
        });

        var octalInteger = Sequential(static s =>
        {
            s.Expect(MatchChar('0'));
            return ParseUnsignedIntegerBodyWithBase(s, 8, _octalDigit, 0U);
        });

        return First(
            decimalInteger,
            CreateHexUnsignedIntegerParser(),
            octalInteger
        );
    }

    public static IParser<char, long> CreateSignedLongParser()
    {
        var decimalInteger = Sequential(static s =>
        {
            var sign = s.TryParse(MatchChar('-')).Success;

            // Get the first digit, which cannot be zero.
            var d = s.Parse(Digit());
            if (d == '0')
                s.Fail(_errorCannotStartWithZero);
            return ParseSignedLongBodyWithBase(s, 10, Digit(), d - '0', sign);
        });

        var octalInteger = Sequential(static s =>
        {
            var sign = s.TryParse(MatchChar('-')).Success;
            s.Expect(MatchChar('0'));
            return ParseSignedLongBodyWithBase(s, 8, _octalDigit, 0, sign);
        });

        return First(
            decimalInteger,
            CreateHexSignedLongParser(),
            octalInteger
        );
    }

    public static IParser<char, ulong> CreateUnsignedLongParser()
    {
        var decimalInteger = Sequential(static s =>
        {
            // Get the first digit, which cannot be zero.
            var d = s.Parse(Digit());
            if (d == '0')
                s.Fail(_errorCannotStartWithZero);
            return ParseUnsignedLongBodyWithBase(s, 10, Digit(), (ulong)d - '0');
        });

        var octalInteger = Sequential(static s =>
        {
            s.Expect(MatchChar('0'));
            return ParseUnsignedLongBodyWithBase(s, 8, _octalDigit, 0UL);
        });

        return First(
            decimalInteger,
            CreateHexUnsignedLongParser(),
            octalInteger
        );
    }

    private static long ParseSignedHexLong(SequentialState<char> s, int maxDigits)
    {
        var sign = s.TryParse(MatchChar('-')).Success;
        var longValue = (long)ParseUnsignedHexLong(s, maxDigits);
        return sign ? -longValue : longValue;
    }

    private static ulong ParseUnsignedHexLong(SequentialState<char> s, int maxDigits)
    {
        s.Expect('0');
        s.Expect('x');

        var digit = HexadecimalDigit();
        var firstDigit = s.Parse(digit);

        ulong value = ConvertHexDigit(firstDigit);
        for (int i = 1; i < maxDigits; i++)
        {
            var d = s.TryParse(digit);
            if (!d.Success)
                break;
            value = (value << 4) + ConvertHexDigit(d.Value);
        }

        return value;
    }

    private static int ParseSignedIntegerBodyWithBase(SequentialState<char> s, int numberBase, IParser<char, char> parseChar, int startValue, bool sign)
    {
        int maxBeforeLastDigit = int.MaxValue / numberBase;
        int minBeforeLastDigit = int.MinValue / numberBase;
        var value = AdjustForSign(startValue, sign);

        // Keep getting more digits so long as we stay below maxint and there are more digits
        // to get.
        while (true)
        {
            // We can add another digit IF the current value is less than or equal to
            // maxBeforeLastDigit AND if value<=(int.MaxValue-nextDigit)/base
            if ((!sign && value > maxBeforeLastDigit) || (sign && value < minBeforeLastDigit))
                return value;

            var cp = s.Checkpoint();
            var maybeD = s.TryParse(parseChar);

            // No more digits, so we're done.
            if (!maybeD.Success)
                return value;

            var digitValue = maybeD.Value - '0';
            var nextValue = value * numberBase;
            if ((sign && int.MinValue + digitValue > nextValue) || (!sign && int.MaxValue - digitValue < nextValue))
            {
                cp.Rewind();
                return value;
            }

            value = nextValue + AdjustForSign(digitValue, sign);
        }
    }

    private static uint ParseUnsignedIntegerBodyWithBase(SequentialState<char> s, int numberBase, IParser<char, char> parseChar, uint startValue)
    {
        var maxBeforeLastDigit = uint.MaxValue / numberBase;
        var value = startValue;

        // Keep getting more digits so long as we stay below maxint and there are more digits
        // to get.
        while (true)
        {
            // We can add another digit IF the current value is less than or equal to
            // maxBeforeLastDigit AND if value<=(int.MaxValue-nextDigit)/base
            if (value > maxBeforeLastDigit)
                return value;

            var cp = s.Checkpoint();
            var maybeD = s.TryParse(parseChar);

            // No more digits, so we're done.
            if (!maybeD.Success)
                return value;

            uint digitValue = (uint)(maybeD.Value - '0');
            uint nextValue = value * (uint)numberBase;
            if (uint.MaxValue - digitValue < nextValue)
            {
                cp.Rewind();
                return value;
            }

            value = nextValue + digitValue;
        }
    }

    private static long ParseSignedLongBodyWithBase(SequentialState<char> s, int numberBase, IParser<char, char> parseChar, long startValue, bool sign)
    {
        long maxBeforeLastDigit = long.MaxValue / numberBase;
        long minBeforeLastDigit = long.MinValue / numberBase;
        var value = AdjustForSign(startValue, sign);

        // Keep getting more digits so long as we stay below maxint and there are more digits
        // to get.
        while (true)
        {
            // We can add another digit IF the current value is less than or equal to
            // maxBeforeLastDigit AND if value<=(int.MaxValue-nextDigit)/base
            if ((!sign && value > maxBeforeLastDigit) || (sign && value < minBeforeLastDigit))
                return value;

            var cp = s.Checkpoint();
            var maybeD = s.TryParse(parseChar);

            // No more digits, so we're done.
            if (!maybeD.Success)
                return value;

            var digitValue = maybeD.Value - '0';
            var nextValue = value * numberBase;
            if ((sign && long.MinValue + digitValue > nextValue) || (!sign && long.MaxValue - digitValue < nextValue))
            {
                cp.Rewind();
                return value;
            }

            value = nextValue + AdjustForSign(digitValue, sign);
        }
    }

    private static ulong ParseUnsignedLongBodyWithBase(SequentialState<char> s, int numberBase, IParser<char, char> parseChar, ulong startValue)
    {
        var maxBeforeLastDigit = ulong.MaxValue / (ulong)numberBase;
        var value = startValue;

        // Keep getting more digits so long as we stay below maxint and there are more digits
        // to get.
        while (true)
        {
            // We can add another digit IF the current value is less than or equal to
            // maxBeforeLastDigit AND if value<=(int.MaxValue-nextDigit)/base
            if (value > maxBeforeLastDigit)
                return value;

            var cp = s.Checkpoint();
            var maybeD = s.TryParse(parseChar);

            // No more digits, so we're done.
            if (!maybeD.Success)
                return value;

            ulong digitValue = (ulong)(maybeD.Value - '0');
            ulong nextValue = value * (ulong)numberBase;
            if (ulong.MaxValue - digitValue < nextValue)
            {
                cp.Rewind();
                return value;
            }

            value = nextValue + digitValue;
        }
    }

    private static int AdjustForSign(int value, bool sign)
        => sign ? -value : value;

    public static long AdjustForSign(long value, bool sign)
        => sign ? -value : value;

    private static ulong ConvertHexDigit(char c)
        => c switch
        {
            >= 'a' and <= 'f' => (ulong)(c - 'a' + 10),
            >= 'A' and <= 'F' => (ulong)(c - 'A' + 10),
            _ => (ulong)(c - '0')
        };
}
