using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public static partial class Pratt<TInput, TOutput>
    {
        /// <summary>
        /// Configures a parselet rule for the Pratt parser.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public interface IParseletConfiguration<TValue>
        {
            IParseletConfiguration<TValue> TypeId(int id);

            /// <summary>
            /// The name of this parselet.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            IParseletConfiguration<TValue> Named(string name);

            /// <summary>
            /// The binding power (precidence) of this value to the value on the left. For a
            /// left-associative token, the LeftBindingPower should be higher than the
            /// RightBindingPower. For a right-associative token, the RightBindingPower should be
            /// higher. By default this value is 0.
            /// </summary>
            /// <param name="lbp"></param>
            /// <returns></returns>
            IParseletConfiguration<TValue> LeftBindingPower(int lbp);

            /// <summary>
            /// The binding power (precidence) of this value to the value on the right. For a
            /// left-associative token, the LeftBindingPower should be higher than the
            /// RightBindingPower. For a right-associative token, the RightBindingPower should be
            /// higher. By default this value is the same as the LeftBindingPower value.
            /// </summary>
            /// <param name="rbp"></param>
            /// <returns></returns>
            IParseletConfiguration<TValue> RightBindingPower(int rbp);

            /// <summary>
            /// If this operator interacts with a value on the left, the ProduceLeft callback is
            /// invoked, taking both the left value and the current value, and producing a new
            /// output. This callback may recurse into the parser using the context object, if
            /// additional values are required (for example, an infix operator requiring a right
            /// operands as well).
            /// </summary>
            /// <param name="getLed"></param>
            /// <returns></returns>
            IParseletConfiguration<TValue> ProduceLeft(LedFunc<TValue> getLed);

            /// <summary>
            /// If this operator only interacts with a value on the right side, the ProduceRight
            /// callback is invoked. This callback takes the current value, and allows recursing
            /// into the parser to obtain a right value using the context object.
            /// </summary>
            /// <param name="getNud"></param>
            /// <returns></returns>
            IParseletConfiguration<TValue> ProduceRight(NudFunc<TValue> getNud);
        }

        /// <summary>
        /// Configuration object for a Pratt parser.
        /// </summary>
        public interface IConfiguration
        {
            /// <summary>
            /// Add a parselet.
            /// </summary>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="matcher"></param>
            /// <param name="setup"></param>
            /// <returns></returns>
            IConfiguration Add<TValue>(IParser<TInput, TValue> matcher, Action<IParseletConfiguration<TValue>> setup);
            IConfiguration Add(IParser<TInput, TOutput> matcher);
        }

        private class Configuration : IConfiguration
        {
            public List<IParselet> Parselets { get; }

            public Configuration()
            {
                Parselets = new List<IParselet>();
            }

            public IConfiguration Add<TValue>(IParser<TInput, TValue> matcher, Action<IParseletConfiguration<TValue>> setup)
            {
                var parseletConfig = new ParseletConfiguration<TValue>(matcher);
                setup(parseletConfig);
                var parselet = parseletConfig.Build();
                Parselets.Add(parselet);
                return this;
            }

            public IConfiguration Add(IParser<TInput, TOutput> matcher) => Add(matcher, p => p.ProduceRight((ctx, v) => v.Value));

            public IEnumerable<IParser> GetParsers() => Parselets.Select(p => p.Parser);
        }

        private class ParseletConfiguration<TValue> : IParseletConfiguration<TValue>
        {
            private readonly IParser<TInput, TValue> _matcher;

            private int _typeId;
            private int _lbp;
            private int _rbp;
            private NudFunc<TValue> _getNud;
            private LedFunc<TValue> _getLed;
            private string _name;

            public ParseletConfiguration(IParser<TInput, TValue> matcher)
            {
                _matcher = matcher;
            }

            public IParselet Build()
            {
                return new Parselet<TValue>(
                    _typeId,
                    _matcher,
                    _getNud,
                    _getLed,
                    _lbp,
                    _rbp <= 0 ? _lbp + 1 : _rbp,
                    _name ?? _matcher.Name ?? _typeId.ToString()
                );
            }

            public IParseletConfiguration<TValue> TypeId(int id)
            {
                _typeId = id;
                return this;
            }

            public IParseletConfiguration<TValue> ProduceRight(NudFunc<TValue> getNud)
            {
                _getNud = getNud;
                return this;
            }

            public IParseletConfiguration<TValue> ProduceLeft(LedFunc<TValue> getLed)
            {
                _getLed = getLed;
                return this;
            }

            public IParseletConfiguration<TValue> LeftBindingPower(int lbp)
            {
                _lbp = lbp;
                return this;
            }

            public IParseletConfiguration<TValue> RightBindingPower(int rbp)
            {
                _rbp = rbp;
                return this;
            }

            public IParseletConfiguration<TValue> Named(string name)
            {
                _name = name;
                return this;
            }
        }
    }
}
