using System.Collections;
using System.Reflection;

using PythonNetStubGenerator;

namespace PythonNetStubTool
{
    static class Program
    {
        /// <summary>
        /// Creates stubs for Python.Net
        /// </summary>
        /// <param name="destPath">Path to save the subs to.</param>
        /// <param name="searchPaths">Path to search for referenced assemblies</param>
        /// <param name="targetDlls">Target DLLs</param>
        static int Main(
            DirectoryInfo destPath,
            DirectoryInfo[]? searchPaths = null,
            params FileInfo[] targetDlls)
        {
            if (searchPaths != null)
            {
                foreach (var searchPath in searchPaths)
                    Console.WriteLine($"search path {searchPath}");
            }

            
            foreach (var assemblyPath in targetDlls)
            {
                if (!assemblyPath.Exists)
                {
                    Console.WriteLine($"error: can not find {assemblyPath}");
                    return -1;
                }

            }
            
            Console.WriteLine($"building stubs...");

            try
            {
                var dest = StubBuilder.BuildAssemblyStubs(destPath, targetDlls, searchPaths);
                Console.WriteLine($"stubs saved to {dest}");
                return 0;
            }
            catch (Exception sgEx)
            {
                Console.WriteLine($"error: failed generating stubs | {sgEx.Message}");
                throw;
            }
        }
    }
}
