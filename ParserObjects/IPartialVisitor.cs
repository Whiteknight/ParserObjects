namespace ParserObjects;

public interface IPartialVisitor<TState>
{
    bool TryAccept(IParser parser, TState state);
}
