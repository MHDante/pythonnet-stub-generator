using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PythonNetStubGenerator;
using System;
using System.IO;
using System.Linq;

namespace PythonNetStubTask
{
    public class PythonNetStubTask : Task
    {
        [Required]
        public string DestPath { get; set; }

        [Required]
        public string[] SourceDlls { get; set; }

        public string[] AdditionalSearchPaths { get; set; }

        public override bool Execute()
        {
            var destPath = new DirectoryInfo(DestPath);
            var sourceDlls = SourceDlls.Select(it => new FileInfo(it)).ToArray();
            var directoryPaths = AdditionalSearchPaths?.Select(it => new DirectoryInfo(it)).ToArray();

            try
            {
                Log.LogMessage(MessageImportance.High, "Generating Stubs for " + destPath.FullName);
                StubBuilder.BuildAssemblyStubs(destPath, sourceDlls, directoryPaths);
                Log.LogMessage(MessageImportance.High, "Done");

            }
            catch (Exception e)
            {
                Log.LogError(e.Message);
                return false;
            }
            return true;
        }

    }
}