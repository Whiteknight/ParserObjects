namespace ParserObjects.Pratt
{
    public interface IParseletConfiguration<TInput, TValue, TOutput> : INamed
    {
        IParseletConfiguration<TInput, TValue, TOutput> TypeId(int id);

        /// <summary>
        /// If this operator interacts with a value on the left, the ProduceLeft callback is
        /// invoked, taking both the left value and the current value, and producing a new
        /// output. This callback may recurse into the parser using the context object, if
        /// additional values are required (for example, an infix operator requiring a right
        /// operands as well).
        /// </summary>
        /// <param name="lbp"></param>
        /// <param name="rbp"></param>
        /// <param name="getLed"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed);

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

        IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(int lbp, LedFunc<TInput, TValue, TOutput> getLed);
    }
}
