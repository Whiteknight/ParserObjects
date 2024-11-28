using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

// Allows recursion back into the regex engine.
public delegate bool TestFunc(CaptureCollection captures, List<IState> states, ISequence<char> input, bool canBacktrack);
