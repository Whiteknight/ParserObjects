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

This may be a savings in situations where you are parsing, rewinding, and re-parsing the same series of input multiple times. You should benchmark your application to see if it is helpful to cache. Notice that, if you did not create the ParseState with a working cache, the `Cache` parser will do no caching.

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

### Examine Parser

The `Examine` parser allows inserting callbacks before or after any other parser, and is primarily used for debugging. 

```csharp
var parser = inner.Examine(
    before: parseContext => { ... }, 
    after: parseContext => { ... }
);
```

The `parseContext` struct contains references to the `inner` parser, the current parse state and the parse result (in the `after` callback only). 

Either the before or after callbacks can be omitted if not required. If both callbacks are omitted, `parser` will be identical to `innerParser`. 

Exceptions thrown from either callback will not be automatically handled. If you made changes to the parse state in the `before` method and an exception is thrown, those changes will not have a chance to be cleaned up and the parse may be left in an indeterminate state. For this reason it is best not to make modifications to the parse state in the Examine parser (even if it is technically possible to do).

### Context Parser

The `Context` parser allows executing an arbitrary callback before and after invoking an inner parser, with access to the parse state. This is used to make changes to the state before, and cleanup those changes after.

```csharp
var parser = Context(innerParser, 
    before: state => { ... },
    after: state => { ... }
);
```

Either the before or after callbacks can be omitted if not required. If both callbacks are ommitted, `parser` will be identical to `innerParser`. 

The Context parser handles errors in the following way:
1. If an exception is thrown in the `before` callback, the exection will bubble up to be caught by the user, but the parse should be in a consistent state.
2. If an exception is thrown by the `innerParser`, the `after` callback will be executed to clean the parse state, and then the exception will bubble up.
3. If an exception is thrown by the `after` callback, the exception will bubble up and the parse will be in an inconsistent state.

It is a good idea to try not to throw exceptions from these or any other callback methods, but if one gets thrown anyway a best-effort attempt will be made to cleanup the parse state before going to the exception handler.

**Notice**: The `Context` parser is very similar to the `Examine` parser in structure and functionality. However the Context parser is intended to make actual modifications to the parse and cleanup afterwards, in production environments. The Examine parser is more intended for debugging purposes and doesn't offer the same guarantees as the Context parser. You *can modify the parse state* in the Examine parser callbacks but you shouldn't. Making changes to the parse state and cleaning them up afterwards is the intended purpose of `Context()`. 

## State Data Management

The parse state can hold some contextual data which can be inserted and read at any point in the parse. Any callback which takes an `IParseState<TInput>` can set or read contextual data. You can use this data in, for example, the `Create` parser to create at parse-time a parser to match a value which is not known until parse time. 

All data stored in the contextual data store must have a string name and be accessed by type. Attempting to access a piece of data with the wrong name or the wrong data type will result no no data being returned.

There are several Parsers which operate on State data, and state data can be accessed from any other parser whose callback contains an `IParseState` reference.

### GetData Parser

The `GetData` parser searches for data in the contextual data store by name and type.

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
