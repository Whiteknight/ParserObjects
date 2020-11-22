namespace ParserObjects.Utility
{
    public interface IDataStore
    {
        (bool Success, T Value) Get<T>(string name);
        void Set<T>(string name, T value);
    }
}
