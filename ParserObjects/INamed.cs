namespace ParserObjects;

/// <summary>
/// A entity which may have a name. Names are used primarily for debugging, tracing and
/// auditing purposes, and also for certain find/replace operations.
/// </summary>
public interface INamed
{
    /// <summary>
    /// Gets or Sets the name of the object. This value is only used for bookkeeping, debugging
    /// and tracing only. It does not have any effect on parsing or other processes.
    /// </summary>
    string Name { get; set; }
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
        nameable.Name = name;
        return nameable;
    }
}
