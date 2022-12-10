namespace ParserObjects;

public interface IPartialVisitor<in TState>
{
    bool TryAccept(IParser parser, TState state);
}
