namespace ParserObjects;

/// <summary>
/// A visitor which handles some parser types.
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IPartialVisitor<in TState>
{
    /// <summary>
    /// Attempt to accept the given parser. Returns true if accepted. Return false if this visitor
    /// does not accept parsers of this type.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    bool TryAccept(IParser parser, TState state);
}
