using System;
using System.Collections.Generic;
using ParserObjects.Parsers;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput, TOutput> Create<TOutput>(Func<ParseState<TInput>, IParser<TInput, TOutput>> create)
            => new CreateParser<TInput, TOutput>(create);

        public static IParser<TInput, TValue> GetData<TValue>(string name)
            => Function<TValue>((t, success, fail) =>
            {
                var (has, value) = t.Data.Get<TValue>(name);
                if (has)
                    return success(value);
                return fail($"State data '{name}' does not exist");
            });

        public static IParser<TInput, TValue> SetData<TValue>(string name, TValue value)
            => Function<TValue>((t, success, fail) =>
            {
                t.Data.Set(name, value);
                return success(value);
            });

        public static IParser<TInput, TOutput> SetResultData<TOutput>(IParser<TInput, TOutput> p, string name)
            => new TransformResultParser<TInput, TOutput, TOutput>(p, (t, r) =>
            {
                if (!r.Success)
                    return r;

                t.Data.Set(name, r.Value);
                return r;
            });

        public static IParser<TInput, TOutput> SetResultData<TOutput, TValue>(IParser<TInput, TOutput> p, string name, Func<TOutput, TValue> getValue)
            => new TransformResultParser<TInput, TOutput, TOutput>(p, (t, r) =>
            {
                if (!r.Success)
                    return r;

                var value = getValue(r.Value);
                t.Data.Set(name, value);
                return r;
            });

        public static IParser<TInput, TOutput> RecurseData<TOutput>(IParser<TInput, TOutput> inner, Dictionary<string, object> values = null)
            => Function<TOutput>((t, success, fail) =>
            {
                try
                {
                    // We hide push/pop methods behind the interface, for now, because we don't
                    // want downstream users monkeying with push/pop and unbalancing the whole thing
                    (t.Data as CascadingKeyValueStore)?.PushFrame();
                    if (values != null)
                    {
                        foreach (var kvp in values)
                            t.Data.Set(kvp.Key, kvp.Value);
                    }
                    return inner.Parse(t);
                }
                finally
                {
                    (t.Data as CascadingKeyValueStore)?.PopFrame();
                }
            });

        public static IParser<TInput, TOutput> RecurseData<TOutput>(IParser<TInput, TOutput> inner, string name, object value)
            => RecurseData(inner, new Dictionary<string, object> { { name, value } });
    }
}
