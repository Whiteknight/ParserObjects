namespace ParserObjects;

#pragma warning disable S125
#pragma warning disable S2326

/* Visitors consist of two classes:
 * 1. A Visitor, which serves as a holder for a list of IPartialVisitor<TState>
 * 2. A State, which holds information about the visit and performs the operation.
 * These two can be the same class if desired
 * The real work is done by the IPartialVisitor<TState> which presumably should implement the
 * type-specific logic for the visit
 *
 * The parser will call
 *
 *      visitor.Get<PartialVisitorType>().Accept(this, state);
 *
 * The state, in turn, should have a reference to the visitor, and can call
 *
 *      parser.Visit(this, state);
 *
 * The downstream user can setup the visitor using static global registrations or DI, or whatever
 * mechanism makes sense for them.
 */

/// <summary>
/// A visitor which handles some parser types.
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IPartialVisitor<in TState>
{
    // Marker type which indicates a partial visitor.
    // A partial visitor should have an .Accept() method variant for every parser type which would
    // invoke it.
}

/// <summary>
/// A visitor for the parser graph.
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IVisitor<TState>
{
    TPartial? Get<TPartial>()
        where TPartial : IPartialVisitor<TState>;
}
