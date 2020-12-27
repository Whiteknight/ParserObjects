using System;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// Contains state data and metadata about the progress of the parse. Provides references to
    /// useful objects which may affect the parse.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public sealed class ParseState<TInput> : IParseState<TInput>
    {
        private readonly Action<string> _logCallback;
        private readonly CascadingKeyValueStore _store;

        public ParseState(ISequence<TInput> input, Action<string> logCallback)
        {
            Input = input;
            _store = new CascadingKeyValueStore();
            _logCallback = logCallback;
        }

        /// <summary>
        /// Gets the input sequence being used by the parser.
        /// </summary>
        public ISequence<TInput> Input { get; }

        /// <summary>
        /// Gets the current contextual state data used by the parser.
        /// </summary>
        public IDataStore Data => _store;

        /// <summary>
        /// If a log callback is provided, pass a log message to the callback.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="message"></param>
        public void Log(IParser parser, string message) => _logCallback?.Invoke($"{parser}: {message}");
    }
}
