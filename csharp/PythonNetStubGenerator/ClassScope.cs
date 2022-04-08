using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PythonNetStubGenerator
{
    public class ClassScope : IDisposable
    {
        private static readonly List<ClassScope> ClassScopes = new List<ClassScope>();
        public static ClassScope Current => ClassScopes.LastOrDefault();
        public string PythonClass { get; }
        public Type[] Generics { get; }

        private IndentScope IndentScope { get; set; }

        public ClassScope(string pythonClass, IEnumerable<Type> newGenerics)
        {
            Generics = newGenerics.ToArray();
            PythonClass = pythonClass;
            ClassScopes.Add(this);
        }

        public void EnterIndent() => IndentScope ??= new IndentScope();

        public void Dispose()
        {
            var index = ClassScopes.Count - 1;
            var existing = ClassScopes[index];
            ClassScopes.RemoveAt(index);
            IndentScope?.Dispose();
            if (existing != this) throw new Exception();
        }

        public static string ScopeAccessor =>
            ClassScopes.Count == 0 ? "" :
                string.Join(".", ClassScopes.Select(it => it.PythonClass)) + ".";

        public static IEnumerable<Type> AccessibleGenerics =>
            ClassScopes.SelectMany(it => it.Generics);
    }

}