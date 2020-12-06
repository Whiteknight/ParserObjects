namespace ParserObjects.Pratt
{
    public interface IParseletConfiguration<TInput, TValue, TOutput> : INamed
    {
        IParseletConfiguration<TInput, TValue, TOutput> TypeId(int id);

        /// <summary>
        /// The binding power (precidence) of this value to the value on the left. For a
        /// left-associative token, the LeftBindingPower should be higher than the
        /// RightBindingPower. For a right-associative token, the RightBindingPower should be
        /// higher. By default this value is 0.
        /// </summary>
        /// <param name="lbp"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> LeftBindingPower(int lbp);

        /// <summary>
        /// The binding power (precidence) of this value to the value on the right. For a
        /// left-associative token, the LeftBindingPower should be higher than the
        /// RightBindingPower. For a right-associative token, the RightBindingPower should be
        /// higher. By default this value is the same as the LeftBindingPower value.
        /// </summary>
        /// <param name="rbp"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> RightBindingPower(int rbp);

        /// <summary>
        /// If this operator interacts with a value on the left, the ProduceLeft callback is
        /// invoked, taking both the left value and the current value, and producing a new
        /// output. This callback may recurse into the parser using the context object, if
        /// additional values are required (for example, an infix operator requiring a right
        /// operands as well).
        /// </summary>
        /// <param name="getLed"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(LedFunc<TInput, TValue, TOutput> getLed);

        /// <summary>
        /// If this operator only interacts with a value on the right side, the ProduceRight
        /// callback is invoked. This callback takes the current value, and allows recursing
        /// into the parser to obtain a right value using the context object.
        /// </summary>
        /// <param name="getNud"></param>
        /// <returns></returns>
        IParseletConfiguration<TInput, TValue, TOutput> ProduceRight(NudFunc<TInput, TValue, TOutput> getNud);
    }
}
