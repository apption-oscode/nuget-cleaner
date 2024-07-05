using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetCleaner.CLI
{
    internal class CleanerOptions
    {
        [Option('s', "add-source", Required = false, HelpText = "Set nuget package source")]
        public string PackageSource { get; set; } = "https://api.nuget.org/v3/index.json";

        [Option("api-key", Required = true, HelpText = "Api Key for nuget server")]
        public string ApiKey { get; set; } = null!;

        [Option('k', "keep", Required = false, HelpText = "Set the number of versions to keep", Default = 3)]
        public int VersionsToKeep { get; set; }

        [Option('y', "confirm", Required = false, Default = false, HelpText = "Confirm the deletion and do not ask for confirmation")]
        public bool Confirm { get; set; } = false;

        [Value(0, MetaName = "package", HelpText = "Package Identified")]
        public string PackageId { get; set; } = null!;
    }
}
