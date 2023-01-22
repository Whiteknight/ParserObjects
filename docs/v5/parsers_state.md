# State-Management Parsers

There are a number of parsers which can be used to control the parse state.

## Caching

The `IParseState<TInput>` contains a cache which can hold a pre-existing `IResult` from a given parser at a given `Location` in the input sequence. 

```csharp
var parseState = new ParseState<TInput>(input, s => { }, cache);
```

By default caching is disabled, but you can create your ParseState with a functioning cache in order to get the benefits.

### Cache Parser

The `Cache` parser will cache the results of the inner parser, at the current input location:

```csharp
var parser = Cache(innerParser);
```

This may be a savings in situations where you are parsing, rewinding, and re-parsing the same series of input multiple times. You should benchmark your application to see if it is helpful to cache.

### Creating Caches

You can create caches using factory methods.

```csharp
using static ParserObjects.Caches;
```

The default implementation which does no caching is the `NullCache`:

```csharp
var cache = NullCache();
```

You can also create a cache from an `Microsoft.Extensions.Caching.Memory.IMemoryCache`, either creating a new one or using an existing one:

```csharp
var cache = InMemoryCache();
var cache = InMemoryCache(existingCache);
```

## State Management

### Context Parser

The `Context` parser allows executing an arbitrary callback before and after invoking an inner parser. 

## State Data Management

The parse state can hold some contextual data which can be inserted and read at any point in the parse. Any callback which takes an `IParseState<TInput>` can set or read contextual data. You can use this data in, for example, the `Create` parser to create at parse-time a parser to match a value which is not known until parse time. 

All data stored in the contextual data store must have a string name and be accessed by type. 

### GetData Parser

The `GetData` parser searches for data in the contexual data store by name and type.

```csharp
var parser = GetData<MyDataType>("name");
```

If the name does not exist in the store, or if the object with that name does not match the given type, a failure result is returned.

### SetData Parser

The `SetData` parser allows you to set a data value during the parse.

```csharp
var parser = SetData<MyDataType>("name", new MyDataType());
```

The SetData parser will create the entry with the given name if it does not exist, or will overwrite an existing value if it does exist.

### SetResultData Parser

The `SetResultData` parser sets the result value from a given parser into the contextual data store.

```csharp
var parser = SetResultData(innerParser, "name");
```

The result value is only set if the inner parser succeeds. If the inner parser fails no data is stored, and a previous value for that name may remain.

### DataContext Parser

The `DataContext` pushes a new frame onto the contextual data store and executes the inner parser with that frame. Frames form a FILO queue. Data stored with the `SetData` or `SetResultData` parsers will be stored in the current data frame only, and when that frame is popped the values in that frame are lost. In this way you can have data with the same name have different values at different points in the parse.

```csharp
var parser = DataContext(innerParser);
```

There are also variants where you can seed the new DataContext when you create it:

```csharp
var parser = DataContext(innerParser, "name", value);
var parser = DataContext(innerParser, new Dictionary<string, object>
    { "name1", value1 },
    { "name2", value2 }
})
```

The seeded values can be read with `GetData` and overwritten with `SetData` or `SetResultData` like normal.
