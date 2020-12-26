using System.Collections.Generic;
using System.Linq;
using ParserObjects.Gll;
using ParserObjects.Parsers;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Gll
{
    public class ParseTests
    {
        private IReadOnlyList<IMatch> Execute<TOutput>(string s, IGllParser<char, TOutput> parser)
        {
            var input = new StringCharacterSequence(s);
            var engine = Engine.Instance;
            return engine.Execute(input, parser);
        }

        [Test]
        public void Any_Test()
        {
            var parser = new AnyParser<char>();
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeTrue();
            result.GetValue<char>().Value.Should().Be('a');
        }

        [Test]
        public void Any_Fail_End()
        {
            var parser = new AnyParser<char>();
            var result = Execute("", parser).Single();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Match_Test()
        {
            var parser = new MatchPredicateParser<char>(c => c == 'a');
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeTrue();
            result.GetValue<char>().Value.Should().Be('a');
        }

        [Test]
        public void Match_Fail_Wrong()
        {
            var parser = new MatchPredicateParser<char>(c => c == 'a');
            var result = Execute("Xbc", parser).Single();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Match_Fail_End()
        {
            var parser = new MatchPredicateParser<char>(c => c == 'a');
            var result = Execute("", parser).Single();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Alternates_Test()
        {
            var parser = new AlternativesGllParser<char, char>(
                new MatchPredicateParser<char>(c => c == 'a'),
                new AnyParser<char>()
            );
            var results = Execute("abc", parser);
            results.Count.Should().Be(2);
            results[0].Success.Should().BeTrue();
            results[0].GetValue<char>().Value.Should().Be('a');
            results[1].Success.Should().BeTrue();
            results[1].GetValue<char>().Value.Should().Be('a');
        }

        [Test]
        public void Rule_2_Test()
        {
            var parser = new RuleGllParser<char, string>(
                new[]
                {
                    new AnyParser<char>(),
                    new AnyParser<char>()
                },
                (r) => $"{r[0]}:{r[1]}"
            );
            var results = Execute("abc", parser);
            results.Count.Should().Be(1);
            results[0].Success.Should().BeTrue();
            results[0].GetValue<string>().Value.Should().Be("a:b");
        }

        [Test]
        public void Optional_Success()
        {
            var parser = new OptionalGllParser<char, char>(
                new MatchPredicateParser<char>(c => c == 'a')
            );
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeTrue();
            result.State.StartCheckpoint.Consumed.Should().Be(1);
            result.GetValue<char>().Value.Should().Be('a');
        }

        [Test]
        public void Optional_Fail()
        {
            var parser = new OptionalGllParser<char, char>(
                new MatchPredicateParser<char>(c => c == 'X')
            );
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeTrue();
            result.State.StartCheckpoint.Consumed.Should().Be(0);
            result.GetValue<char>().Value.Should().Be('\0');
        }

        [Test]
        public void PositiveLookahead_Success()
        {
            var parser = new PositiveLookaheadGllParser<char, char>(
                new MatchPredicateParser<char>(c => c == 'a')
            );
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeTrue();
            result.State.StartCheckpoint.Consumed.Should().Be(0);
            result.GetValue<char>().Value.Should().Be('a');
        }

        [Test]
        public void PositiveLookahead_Fail()
        {
            var parser = new PositiveLookaheadGllParser<char, char>(
                new MatchPredicateParser<char>(c => c == 'X')
            );
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Failure_Test()
        {
            var parser = new FailParser<char, char>();
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Transform_Success()
        {
            var parser = new TransformGllParser<char, char, string>(
                new AnyParser<char>(),
                c => $"X{c}X"
            );
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeTrue();
            result.State.StartCheckpoint.Consumed.Should().Be(1);
            result.GetValue<string>().Value.Should().Be("XaX");
        }

        [Test]
        public void Transform_Failure()
        {
            var parser = new TransformGllParser<char, char, string>(
                new MatchPredicateParser<char>(c => c == 'X'),
                c => $"X{c}X"
            );
            var result = Execute("abc", parser).Single();
            result.Success.Should().BeFalse();
            result.State.StartCheckpoint.Consumed.Should().Be(0);
        }
    }
}
