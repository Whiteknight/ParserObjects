using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public delegate bool TestFunc(CaptureCollection captures, List<IState> states, ISequence<char> input);
