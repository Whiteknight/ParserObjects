﻿using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Invokes a delegate to perform the parse
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FuncParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<ISequence<TInput>, IResult<TOutput>> _func;

        public FuncParser(Func<ISequence<TInput>, IResult<TOutput>> func)
        {
            Assert.ArgumentNotNull(func, nameof(func));
            _func = func;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ISequence<TInput> t)
        {
            var window = t.Window();
            try
            {
                var result = _func(window);
                if (!result.Success)
                    window.Rewind();
                return result;
            }
            catch
            {
                window.Rewind();
                return Result.Fail<TOutput>(window.CurrentLocation);
            }
        }

        public IResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
