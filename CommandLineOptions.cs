using com.sun.org.apache.xerces.@internal.impl.xs.identity;
using CommandLine;
using CommandLine.Text;
using NUnit.Framework;
using Org.Kevoree.Log.Api;

namespace Org.Kevoree.Core.Bootstrap
{
    internal class CommandLineOptions
    {
        [Option('n', "node.name", Required = false, DefaultValue = "node0", HelpText = "Dotnet main node name")]
        public string NodeName { get; set; }

        [Option('p', "script.path", Required = false, HelpText = "path to a startup model (json or KevScript)")]
        public string ScriptPath { get; set; }

        [Option("nuget.local.repository.path", Required = false, HelpText = "Nuget local repository.")]
        public string NugetLocalRepositoryPath { get; set; }

        [Option("nuget.repository.url", DefaultValue = "https://packages.nuget.org/api/v2", Required = false,
            HelpText = "Nuget remote repository.")]
        public string NugetRepositoryUrl { get; set; }

        [Option("kevoree.registry.url", DefaultValue = "http://registry.kevoree.org", Required = false,
            HelpText = "Kevoree remote registry.")]
        public string KevoreeRegistryUrl { get; set; }

        [Option("kevoree.log.level", DefaultValue = Level.Info, Required = false, HelpText = "Instance log level.")]
        public Level LogLevel { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}