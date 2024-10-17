using System.CodeDom.Compiler;

namespace ParserObjects.CodeGen
{
    public static class StringBuilderExtensions
    {
        public static void TArgsList(this IndentedTextWriter wr, int num)
        {
            wr.Write("T1");
            for (int i = 2; i <= num; i++)
            {
                wr.Write(", T");
                wr.Write(i);
            }
        }

        public static void PList(this IndentedTextWriter wr, int num)
        {
            wr.Write("p1");
            for (int i = 2; i <= num; i++)
            {
                wr.Write(", p");
                wr.Write(i);
            }
        }

        public static void RValueList(this IndentedTextWriter wr, int num)
        {
            wr.Write("r1.Value");
            for (int i = 2; i <= num; i++)
            {
                wr.Write(", r");
                wr.Write(i);
                wr.Write(".Value");
            }
        }
    }
}
