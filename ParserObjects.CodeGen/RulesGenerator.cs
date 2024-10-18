﻿using System.CodeDom.Compiler;
using System.IO;

namespace ParserObjects.CodeGen
{
    internal class RulesGenerator
    {
        public string GetRulesDefs()
        {
            var inner = new StringWriter();
            var wr = new IndentedTextWriter(inner);
            wr.WriteLine("// <auto-generated/>");
            wr.WriteLine("using System;");
            wr.WriteLine();
            wr.WriteLine("namespace ParserObjects;");
            wr.WriteLine();
            wr.WriteLine("public static partial class Parsers<TInput>");
            wr.WriteLine("{");
            wr.Indent++;
            for (int num = 2; num <= 9; num++)
                GenerateRuleMethod(num, wr);

            wr.Indent--;
            wr.WriteLine("}");
            return inner.ToString();
        }

        private void GenerateRuleMethod(int num, IndentedTextWriter wr)
        {
            if (num > 2)
                wr.WriteLine();
            WriteRuleMethodSignature(num, wr);
            WriteMethodParameters(num, wr);
            wr.WriteLine(")");
            wr.WriteLine("{");
            wr.Indent++;

            wr.WriteLine("return Internal.Parsers.Function<TInput, TOutput>.Create(");
            wr.Indent++;

            WriteDefineDataTupleArgument(num, wr);
            wr.WriteLine(",");

            WriteStaticParseMethod(num, wr);
            wr.WriteLine(",");

            // Third the static match method
            WriteStaticMatchMethod(num, wr);
            wr.WriteLine(",");

            // Fourth, the Description/format
            WriteFormatString(num, wr);
            wr.WriteLine(",");

            // Fifth, list of child parsers
            WriteListOfChildParsers(num, wr);
            wr.WriteLine();
            wr.Indent--;
            wr.WriteLine(");");
            wr.Indent--;
            wr.WriteLine("}");
        }

        private static void WriteFormatString(int num, IndentedTextWriter wr)
        {
            wr.Write("\"(%0");
            for (int i = 2; i <= num; i++)
                wr.Write($" %{i - 1}");
            wr.Write(")\"");
        }

        private static void WriteListOfChildParsers(int num, IndentedTextWriter wr)
        {
            wr.Write("new IParser[] { ");
            wr.PList(num);
            wr.Write(" }");
        }

        private static void WriteStaticMatchMethod(int num, IndentedTextWriter wr)
        {
            wr.WriteLine("static (state, args) =>");
            wr.WriteLine("{");
            wr.Indent++;

            wr.Write("var (");
            wr.PList(num);
            wr.WriteLine(", produce) = args;");

            wr.WriteLine("var cp = state.Input.Checkpoint();");

            wr.WriteLine("var ok = p1.Match(state)");
            wr.Indent++;
            for (int i = 2; i <= num - 1; i++)
                wr.WriteLine($"&& p{i}.Match(state)");
            wr.WriteLine($"&& p{num}.Match(state);");
            wr.Indent--;

            wr.WriteLine("if (!ok)");
            wr.Indent++;
            wr.WriteLine("cp.Rewind();");
            wr.Indent--;

            wr.WriteLine("return ok;");
            wr.Indent--;
            wr.Write("}");
        }

        private static void WriteStaticParseMethod(int num, IndentedTextWriter wr)
        {
            // Second, the static parse method
            wr.WriteLine("static (state, args, resultFactory) =>");
            wr.WriteLine("{");

            wr.Indent++;
            wr.Write("var (");
            wr.PList(num);
            wr.WriteLine(", produce) = args;");

            for (int i = 1; i <= num; i++)
            {
                wr.WriteLine($"var r{i} = p{i}.Parse(state);");
                wr.WriteLine($"if (!r{i}.Success)");
                wr.Indent++;
                wr.WriteLine($"return resultFactory.Failure(r{i}.ErrorMessage, parser: p{i});");
                wr.Indent--;
            }

            wr.Write("var result = produce(");
            wr.RValueList(num);
            wr.WriteLine(");");

            wr.WriteLine("return resultFactory.Success(result);");
            wr.Indent--;

            wr.Write("}");
        }

        private static void WriteDefineDataTupleArgument(int num, IndentedTextWriter wr)
        {
            wr.Write("(");
            wr.PList(num);
            wr.Write(", produce)");
        }

        private static void WriteMethodParameters(int num, IndentedTextWriter wr)
        {
            wr.Indent++;
            for (int i = 1; i <= num; i++)
                wr.WriteLine($"IParser<TInput, T{i}> p{i},");

            wr.Write("Func<");
            wr.TArgsList(num);
            wr.Write(", TOutput> produce");
            wr.Indent--;
        }

        private static void WriteRuleMethodSignature(int num, IndentedTextWriter wr)
        {
            wr.Write("public static partial IParser<TInput, TOutput> Rule<");
            wr.TArgsList(num);
            wr.WriteLine(", TOutput>(");
        }
    }
}
