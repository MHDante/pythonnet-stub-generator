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
        /// <param name="targetDlls">Target DLLsz</param>
        static int Main(
            DirectoryInfo destPath,
            string targetDlls,
            DirectoryInfo[]? searchPaths = null
            )
        {
            if (searchPaths != null)
            {
                foreach (var searchPath in searchPaths)
                    Console.WriteLine($"search path {searchPath}");
            }

            var infos = new List<FileInfo>();
            foreach (var pathStr in targetDlls.Split(','))
            {
                var assemblyPath = new FileInfo(pathStr);
                if (!assemblyPath.Exists)
                {
                    Console.WriteLine($"error: can not find {assemblyPath}");
                    return -1;
                }
                infos.Add(assemblyPath);

            }
            
            Console.WriteLine($"building stubs...");

            try
            {
                var dest = StubBuilder.BuildAssemblyStubs(destPath, infos.ToArray(), searchPaths);
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
