using System.Text;

namespace ParserObjects.CodeGen
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder TArgsList(this StringBuilder sb, int num)
        {
            sb.Append("T1");
            for (int i = 2; i <= num; i++)
                sb.Append(", T").Append(i);
            return sb;
        }

        public static StringBuilder PList(this StringBuilder sb, int num)
        {
            sb.Append("p1");
            for (int i = 2; i <= num; i++)
                sb.Append(", p").Append(i);
            return sb;
        }

        public static StringBuilder RValueList(this StringBuilder sb, int num)
        {
            sb.Append("r1.Value");
            for (int i = 2; i <= num; i++)
                sb.Append(", r").Append(i).Append(".Value");
            return sb;
        }
    }
}
