using System;
using System.Collections.Generic;
using System.Text;

namespace PythonNetStubGenerator
{
    
    public static class StringBuilderExtensions
    {
        public static StringBuilder Indent(this StringBuilder sb)
        {
            for (var i = 0; i < IndentScope.IndentLevel; i++) sb.Append("    ");
            return sb;
        }

        public static string CommaJoin(this IEnumerable<string> strings) => string.Join(", ", strings);
    }

    public class IndentScope: IDisposable
    {
        public static int IndentLevel;
        public IndentScope() => IndentLevel++;
        public void Dispose() => IndentLevel--;
    }
    
}