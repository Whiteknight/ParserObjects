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
        {
            var pType = parser.GetType();
            var pTypeName = pType.Name;
            if (pType.DeclaringType != null)
            {
                if (pTypeName == "Parser" || pTypeName == "SingleParser" || pTypeName == "MultiParser")
                    return $"<{pType.DeclaringType.Name} Id={parser.Id}>";

                return $"<{pType.DeclaringType.Name}.{pType.Name} Id={parser.Id}>";
            }

            return $"<{pType.Name} Id={parser.Id}>";
        }

        var objType = named.GetType();
        if (objType.DeclaringType != null)
            return $"<{objType.DeclaringType.Name}.{objType.Name}>";

        return $"<{objType.Name}>";
    }

    public static string ToString(string type, string name, int id)
    {
        if (!string.IsNullOrEmpty(name))
            return $"<{name}>";

        return $"<{type} Id={id}>";
    }
}
