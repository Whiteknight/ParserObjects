using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Gll
{
    public class Engine
    {
        public static Engine Instance = new Engine();

        private record WorkItem(IState State, Func<bool>? Condition);

        private Engine()
        {
        }

        public IReadOnlyList<IMatch> Execute<TInput>(ISequence<TInput> input, IGllParser<TInput> firstParser)
        {
            Exception? currentException = null;
            var unresolvedStates = new Dictionary<string, IState>();
            var workQueue = new PriorityQueue<WorkItem>();

            void Schedule(IState state, Func<bool> condition)
            {
                // TODO: We should make sure we're not pushing a duplicate state, or find a way
                // to cache states so we can reuse a previous one instead of creating new.
                unresolvedStates.Add(state.Id, state);
                state.ResultContinuation.Then(_ =>
                {
                    unresolvedStates.Remove(state.Id);
                });
                var workItem = new WorkItem(state, condition ?? (() => true));
                workQueue.Add(workItem, state.Depth);
            }

            void FlushUnresolvedState()
            {
                if (unresolvedStates.Count == 0)
                    return;

                var states = unresolvedStates
                    .Select(kvp => kvp.Value)
                    .OrderByDescending(s => s.Depth)
                    .ToList();
                var deepestState = states.First();
                deepestState.AddFailure("Forcing failure of deepest unresolved state");
                Debug.Assert(!unresolvedStates.ContainsKey(deepestState.Id), "Deepest unresolved state should be removed");
            }

            var firstState = State<TInput>.New(input, firstParser, e => { currentException = e; }, Schedule);
            Schedule(firstState, () => true);

            while (unresolvedStates.Count > 0)
            {
                while (!workQueue.IsEmpty && !firstState.IsComplete && currentException == null)
                {
                    var itemOption = workQueue.GetNext();
                    Debug.Assert(itemOption.Success, "The work queue should not be empty, so we shouldn't get failure here");

                    var (state, condition) = itemOption.Value;
                    if (!condition())
                        continue;
                    state.Execute();
                }

                if (currentException != null)
                    throw currentException;
                FlushUnresolvedState();
            }

            return firstState.ResultPromise.AllValues;
        }
    }
}
