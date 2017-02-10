using System;
using java.io;
using org.kevoree;
using org.kevoree.kevscript;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Marshalled;
using Org.Kevoree.Log.Api;
using Console = System.Console;
using File = System.IO.File;
using Parser = CommandLine.Parser;
using java.lang;

namespace Org.Kevoree.Core.Bootstrap
{
    public class Bootstrap
    {
        private static readonly KevoreeCoreBean Core = new KevoreeCoreBean();
        public KevoreeCLKernel Kernel;
        private readonly KevScriptEngine _kevScriptEngine = new KevScriptEngine("http://registry.kevoree.org");


        public Bootstrap(string nodeName, string kevoreeRegistryUrl, string nugetLocalRepositoryPath,
            string nugetRepositoryUrl, Level logLevel)
        {
            Kernel = new KevoreeCLKernel(nodeName, nugetLocalRepositoryPath, nugetRepositoryUrl);

            /* WARNING : Really hacky*/
            java.lang.System.setProperty("kevoree.registry", kevoreeRegistryUrl);

            Core.setNodeName(nodeName);
            Core.initLog(logLevel);
            Kernel.SetCore(Core);
            Core.setBootstrapService(Kernel);
            Core.start();
        }

        private static void Main(string[] args)
        {
            var options = new CommandLineOptions();

            if (Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    var boot = new Bootstrap(options.NodeName, options.KevoreeRegistryUrl,
                        options.NugetLocalRepositoryPath, options.NugetRepositoryUrl, Level.Debug);
                    if (options.ScriptPath == null)
                    {
                        /*
                         * A default model is loaded.
                         * It is composed of a WSGroup and a Dotnet Node, attached together.
                         * The Dotnet Node name is "nodeName".
                         * The WSGroup listen on port 9000.
                         */
                        var defaultKevScript = string.Format(@"add {0} : DotnetNode/LATEST/LATEST
add sync : WSGroup/LATEST/LATEST
attach {0} sync", options.NodeName);
                        boot.LoadKevScript(defaultKevScript, x => Core.getLogger().Warn("Bootstrap completed"));
                    }
                    else
                    {
                        /*
                         * We try to load scriptPath file and guess it's content by extention (.json for a json model or .kev for a kev script model).
                         */
                        boot.LoadScript(options.ScriptPath);
                    }
                }
                catch (java.lang.Exception e)
                {
                    Core.getLogger().Error(e.ToString());
                }
            }
        }

        private void LoadKevScript(string defaultKevScript, UpdateCallback callback)
        {
            var emptyModel = initialModel();
            Core.getFactory().root(emptyModel);
            Core.getLogger().Debug(defaultKevScript);
            try
            {
				//Console.WriteLine(defaultKevScript);
                _kevScriptEngine.execute(defaultKevScript, emptyModel);
            }
            catch (java.lang.Exception e)
            {
                throw new System.Exception(e.getMessage());
            }

			Core.update(new ContainerRootMarshalled(emptyModel), callback, "/");
        }

        private void LoadScript(string scriptPath)
        {
            if (scriptPath.EndsWith(".json"))
            {
                var jsonLoader = Core.getFactory().createJSONLoader();
                var model = jsonLoader.loadModelFromStream(new FileInputStream(scriptPath)).get(0);
                bootstrap((ContainerRoot)model, applied => Core.getLogger().Warn(applied.ToString()));
            }
            else if (scriptPath.EndsWith(".kev") || scriptPath.EndsWith(".kevs"))
            {
                var content  = File.ReadAllText(scriptPath);
                LoadKevScript(content, x => Core.getLogger().Warn("Bootstrap completed"));
            }
            else
            {
                Console.WriteLine("Unknow file extension [{0}]", scriptPath);
            }
        }

        private void bootstrap(ContainerRoot model, UpdateCallback callback)
        {
            var emptyModel = initialModel();
            Core.getFactory().root(emptyModel);
            var compare = Core.getFactory().createModelCompare();
            compare.merge(emptyModel, model).applyOn(emptyModel);
            Core.update(new ContainerRootMarshalled(emptyModel), callback, "/");
        }

        private ContainerRoot initialModel()
        {
            var emptyModel = Core.getFactory().createContainerRoot();
            return emptyModel;
        }
    }
}