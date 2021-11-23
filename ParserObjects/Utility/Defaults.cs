namespace ParserObjects.Utility;

public static class Defaults
{
    public static void LogMethod(string _)
    {
        // This is a dummy method used when a log method is not supplied by the user
    }

    public static object ObjectInstance { get; } = new object();
}
