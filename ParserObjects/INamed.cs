namespace ParserObjects;

/// <summary>
/// A entity which may have a name. Names are used primarily for debugging, tracing and
/// auditing purposes, and also for certain find/replace operations.
/// </summary>
public interface INamed
{
    /// <summary>
    /// Gets the name of the object. This value is used for bookkeeping, debugging
    /// and tracing only. It does not have any effect on parsing or other processes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Create a new instance with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    INamed SetName(string name);
}

public static class NamedExtensions
{
    /// <summary>
    /// Specify a name for the parser with function syntax.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nameable"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T Named<T>(this T nameable, string name)
         where T : INamed
    {
        return (T)nameable.SetName(name);
    }
}
