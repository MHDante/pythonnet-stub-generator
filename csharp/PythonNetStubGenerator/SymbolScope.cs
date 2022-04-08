using System;
using System.Collections.Generic;

namespace PythonNetStubGenerator
{
    public class SymbolScope : IDisposable
    {

        public readonly List<string> ReservedSymbols;
        public static readonly List<SymbolScope> Scopes = new List<SymbolScope>();
        public readonly string Namespace;

        public SymbolScope(IEnumerable<string> reservedSymbols, string nameSpace)
        {
            Namespace = nameSpace;
            ReservedSymbols = new List<string>(reservedSymbols);
            Scopes.Add(this);
        }

        public void Dispose()
        {
            Scopes.Remove(this);
        }

        public bool HasConflict(string cleanName, string typeNamespace) => 
            typeNamespace != Namespace && ReservedSymbols.Contains(cleanName);
    }
}