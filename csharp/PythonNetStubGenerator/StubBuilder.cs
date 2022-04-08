using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PythonNetStubGenerator
{
    public static class StubBuilder
    {
        private static HashSet<DirectoryInfo> SearchPaths { get; } = new HashSet<DirectoryInfo>();


        public static DirectoryInfo BuildAssemblyStubs(FileInfo targetAssemblyPath, DirectoryInfo destPath, DirectoryInfo[] searchPaths = null)
        {
            // prepare resolver
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            // pick a dll and load
            var assemblyToStub = Assembly.LoadFrom(targetAssemblyPath.FullName);
            SearchPaths.Add(targetAssemblyPath.Directory);

            if (searchPaths != null)
                foreach (var path in SearchPaths)
                    SearchPaths.Add(path);


            foreach (var exportedType in assemblyToStub.GetExportedTypes())
            {
                PythonTypes.AddDependency(exportedType);
            }


            while (true)
            {
                var (nameSpace, types) = PythonTypes.RemoveDirtyNamespace();
                if (nameSpace == null) break;

                // generate stubs for each type
                WriteStub(destPath, nameSpace, types);
            }


            return destPath;
        }

        internal static void WriteStub(DirectoryInfo rootDirectory, string nameSpace, IEnumerable<Type> stubTypes)
        {
            // sort the stub list so we get consistent output over time
            var orderedTypes = stubTypes.OrderBy(it => it.Name);

            var path = nameSpace.Split('.').Aggregate(rootDirectory.FullName, Path.Combine);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "__init__.pyi");

            PythonTypes.ClearCurrent();

            var stubText = StubWriter.GetStub(nameSpace, orderedTypes);


            File.WriteAllText(path, stubText);
        }


        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var parts = args.Name.Split(',');

            var assemblyToResolve = $"{parts[0]}.dll";

            // try to find the dll in given search paths
            foreach (var searchPath in SearchPaths)
            {
                var assemblyPath = Path.Combine(searchPath.FullName, assemblyToResolve);
                if (File.Exists(assemblyPath)) return Assembly.LoadFrom(assemblyPath);
            }

            return null;

        }
    }
}
