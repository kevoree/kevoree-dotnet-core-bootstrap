using System;
using System.Text;
using java.io;
using org.kevoree;
using org.kevoree.kevscript;
using org.kevoree.modeling.api.json;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Marshalled;
using Org.Kevoree.Core.Microkernel;
using Console = System.Console;
using Parser = CommandLine.Parser;

namespace Org.Kevoree.Core.Bootstrap
{
    public class Bootstrap
    {
        private static readonly JSONModelLoader jsonLoader;
        private readonly KevoreeCoreBean core = new KevoreeCoreBean();
        public KevoreeCLKernel kernel;
        private readonly KevScriptEngine kevScriptEngine = new KevScriptEngine();
        private KevoreeKernel microkernel;


        public Bootstrap(KevoreeKernel k, string nodeName, string kevoreeRegistryUrl, string nugetLocalRepositoryPath,
            string nugetRepositoryUrl)
        {
            //xmiLoader = core.getFactory().createXMILoader();
            microkernel = k;
            kernel = new KevoreeCLKernel(this, nodeName, nugetLocalRepositoryPath, nugetRepositoryUrl);


            /* WARNING : Really hacky*/
            java.lang.System.setProperty("kevoree.registry", kevoreeRegistryUrl);

            core.setNodeName(nodeName);
            //this.kernel.setNodeName(nodeName);
            kernel.setCore(core);
            core.setBootstrapService(kernel);
            core.start();
        }

        private static void Main(string[] args)
        {
            var options = new CommandLineOptions();

            if (Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    var boot = new Bootstrap(KevoreeKernel.Self.Value, options.NodeName, options.KevoreeRegistryUrl,
                        options.NugetLocalRepositoryPath, options.NugetRepositoryUrl);
                    if (options.ScriptPath == null)
                    {
                        /*
                         * A default model is loaded.
                         * It is composed of a WSGroup and a Dotnet Node, attached together.
                         * The Dotnet Node name is "nodeName".
                         * The WSGroup listen on port 9000.
                         */
                       /* var defaultKevScript = string.Format(@"add {0} : DotnetNode
                                                            add sync : WSGroup
                                                            attach {0} sync", options.NodeName);*/

                        var defaultKevScript = string.Format(@"add {0} : DotnetNode
add sync : RemoteWSGroup

attach {0} sync

set sync.host = 'ws.kevoree.org'
set sync.path = '/test'", options.NodeName);
                        boot.loadKevScript(defaultKevScript, x => Console.WriteLine("Bootstrap completed"));
                    }
                    else
                    {
                        /*
                         * We try to load scriptPath file and guess it's content by extention (.json for a json model or .kev for a kev script model).
                         */
                        boot.loadScript(options.ScriptPath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void loadKevScript(string defaultKevScript, UpdateCallback callback)
        {
            var emptyModel = initialModel();
            core.getFactory().root(emptyModel);
            InputStream input = new ByteArrayInputStream(Encoding.Unicode.GetBytes(defaultKevScript.ToCharArray()));
            Console.WriteLine(defaultKevScript);
            try
            {
                kevScriptEngine.execute(defaultKevScript, emptyModel);
            }
            catch (java.lang.Exception e)
            {
                //System.Console.WriteLine(e.getMessage());
                throw new Exception(e.getMessage());
            }

            var currentNode = emptyModel.findNodesByID(core.getNodeName());

            // TODO : is it really necessary to deal with network issues here ?

            core.update(new ContainerRootMarshalled(emptyModel), callback, "/");

            //System.Console.WriteLine("Done");
        }

        private void loadScript(string scriptPath)
        {
            if (scriptPath.EndsWith(".json"))
            {
                // TODO : load a json model
                var model = jsonLoader.loadModelFromStream(new FileInputStream(scriptPath)).get(0);
                bootstrap((ContainerRoot)model, applied => Console.WriteLine(applied));
            }
            else if (scriptPath.EndsWith(".kev"))
            {
                // TODO : load a kev script
            }
            else
            {
                // TODO : allowing XMI ?
                Console.WriteLine("Unknow file extension [{0}]", scriptPath);
            }
        }

        private void bootstrap(ContainerRoot model, UpdateCallback callback)
        {
            var emptyModel = initialModel();
            core.getFactory().root(emptyModel);
            var compare = core.getFactory().createModelCompare();
            compare.merge(emptyModel, model).applyOn(emptyModel);
            core.update(new ContainerRootMarshalled(emptyModel), callback, "/");
        }

        private ContainerRoot initialModel()
        {
            var emptyModel = core.getFactory().createContainerRoot();
            return emptyModel;
        }
    }
}