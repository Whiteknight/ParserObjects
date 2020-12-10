namespace ParserObjects.Pratt
{
    /// <summary>
    /// Used to configure a Parselet for the Pratt parser. A parselet is an adaptor over IParser
    /// with metadata necessary for the Pratt algorithm.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IParseletConfiguration<TInput, TValue, TOutput> : INamed
    {
        /// <summary>
        /// Sets a type ID value for this token. The type ID is a user-provided value which is not
        /// used internally by the Pratt algorithm.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> TypeId(int id);

        /// <summary>
        /// If this operator interacts with a value on the left, the ProduceLeft callback is
        /// invoked, taking both the left value and the current value, and producing a new
        /// output. This callback may recurse into the parser using the context object, if
        /// additional values are required (for example, an infix operator requiring a right
        /// operand as well).
        /// </summary>
        /// <param name="lbp"></param>
        /// <param name="rbp"></param>
        /// <param name="getLed"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed);

        /// <summary>
        /// If this operator interacts with a value on the left, the ProduceLeft callback is
        /// invoked, taking both the left value and the current value, and producing a new
        /// output. This callback may recurse into the parser using the context object, if
        /// additional values are required (for example, in infix operator requiring a right
        /// operand as well).
        /// </summary>
        /// <param name="lbp"></param>
        /// <param name="getLed"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(int lbp, LedFunc<TInput, TValue, TOutput> getLed);

        /// <summary>
        /// If this operator only interacts with a value on the right side, the ProduceRight
        /// callback is invoked. This callback takes the current value, and allows recursing
        /// into the parser to obtain a right value using the context object.
        /// </summary>
        /// <param name="getNud"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceRight(NudFunc<TInput, TValue, TOutput> getNud);

        /// <summary>
        /// If this operator only interacts with a value on the right side, the ProduceRight
        /// callback is invoked. This callback takes the current value, and allows recursing
        /// into the parser to obtain a right value using the context object.
        /// </summary>
        /// <param name="rbp"></param>
        /// <param name="getNud"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceRight(int rbp, NudFunc<TInput, TValue, TOutput> getNud);
    }
}
