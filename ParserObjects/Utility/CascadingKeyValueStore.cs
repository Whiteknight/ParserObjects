using System.Collections.Generic;

namespace ParserObjects.Utility
{
    public class CascadingKeyValueStore : IDataStore
    {
        private readonly LinkedList<Dictionary<string, object>> _store;

        public CascadingKeyValueStore()
        {
            _store = new LinkedList<Dictionary<string, object>>();
            _store.AddLast(new Dictionary<string, object>());
        }

        public Option<T> Get<T>(string name)
        {
            var node = _store.Last;
            while (node != null)
            {
                if (node.Value.ContainsKey(name))
                {
                    var value = node.Value[name];
                    if (value is T typed)
                        return new Option<T>(typed);
                    return default;
                }

                node = node.Previous;
            }

            return default;
        }

        public void Set<T>(string name, T value)
        {
            var dict = _store.Last.Value;
            if (dict.ContainsKey(name))
                dict[name] = value;
            else
                dict.Add(name, value);
        }

        public void PushFrame()
        {
            _store.AddLast(new Dictionary<string, object>());
        }

        public void PopFrame()
        {
            // We don't pop the last frame, ever. So if the caller tries, just ignore it
            if (_store.Count > 1)
                _store.RemoveLast();
        }
    }
}
