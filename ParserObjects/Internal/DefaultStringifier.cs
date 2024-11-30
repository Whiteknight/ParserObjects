using System.Diagnostics.CodeAnalysis;

namespace ParserObjects.Internal;

/// <summary>
/// Default logic for implementing INamed.ToString().
/// </summary>
public static class DefaultStringifier
{
    /// <summary>
    /// Attempts to create a string representation of a parser which is sufficiently
    /// helpful but not overly verbose. Use the .Name if provided.
    /// </summary>
    /// <param name="named"></param>
    /// <returns></returns>
    public static string ToString(INamed named)
    {
        if (!string.IsNullOrEmpty(named.Name))
            return $"<{named.Name}>";
        if (named is IParser parser)
            return UnnamedParserToString(parser);
        return UnnamedNonParserToString(named);
    }

    public static string ToString(string type, string name, int id)
        => !string.IsNullOrEmpty(name)
            ? $"<{name}>"
            : $"<{type} Id={id}>";

    // There are no types in the library which implement INamed but not IParser, so this method
    // won't be called in normal use. We keep it for completeness incase a downstream user wants
    // to stringify a non-parser INamed.
    [ExcludeFromCodeCoverage]
    private static string UnnamedNonParserToString(INamed named)
    {
        var objType = named.GetType();
        return objType.DeclaringType != null
            ? $"<{objType.DeclaringType.Name}.{objType.Name}>"
            : $"<{objType.Name}>";
    }

    private static string UnnamedParserToString(IParser parser)
    {
        var pType = parser.GetType();
        if (pType.DeclaringType == null)
            return $"<{pType.Name} Id={parser.Id}>";

        return pType.Name == "Parser" || pType.Name == "SingleParser" || pType.Name == "MultiParser"
            ? $"<{pType.DeclaringType.Name} Id={parser.Id}>"
            : $"<{pType.DeclaringType.Name}.{pType.Name} Id={parser.Id}>";
    }
}
