using System.Collections.Generic;

namespace ParserObjects.Utility
{
    public class PriorityQueue<T>
    {
        private struct Node
        {
            public Node(T value, int priority)
            {
                Value = value;
                Priority = priority;
            }

            public T Value { get; }
            public int Priority { get; }
        }

        private readonly List<Node> _queue;

        // heap-based priority queue implementation from:
        // https://visualstudiomagazine.com/Articles/2012/11/01/Priority-Queues-with-C.aspx?Page=1
        // TODO: Need to put a lot of testing around this queue implementation

        public PriorityQueue()
        {
            _queue = new List<Node>();
        }

        // TODO: Clean up this code to be more readable

        public void Add(T value, int priority)
        {
            var item = new Node(value, priority);

            _queue.Add(item);
            int ci = _queue.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (_queue[ci].Priority >= _queue[pi].Priority)
                    break;
                var temp = _queue[ci];
                _queue[ci] = _queue[pi];
                _queue[pi] = temp;
                ci = pi;
            }
        }

        public bool IsEmpty => _queue.Count == 0;

        public IOption<T> GetNext()
        {
            if (_queue.Count == 0)
                return FailureOption<T>.Instance;

            int li = _queue.Count - 1;
            var frontItem = _queue[0];
            _queue[0] = _queue[li];
            _queue.RemoveAt(li);

            li--;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li)
                    break;
                int rc = ci + 1;
                if (rc <= li && _queue[rc].Priority < _queue[ci].Priority)
                    ci = rc;
                if (_queue[pi].Priority <= _queue[ci].Priority)
                    break;
                var temp = _queue[pi];
                _queue[pi] = _queue[ci];
                _queue[ci] = temp;
                pi = ci;
            }

            return new SuccessOption<T>(frontItem.Value);
        }
    }
}
