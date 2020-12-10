namespace ParserObjects
{
    /// <summary>
    /// Storage for contextual/state data during a parse.
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Get the data value with the given name, cast to type T. If the data does not exist
        /// or is not assignable to T, the call fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        IOption<T> Get<T>(string name);

        /// <summary>
        /// Set a data value with type T to the given name in the current data frame.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void Set<T>(string name, T value);
    }
}
