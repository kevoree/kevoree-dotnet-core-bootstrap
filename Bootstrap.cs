using java.io;
using org.kevoree;
using org.kevoree.kevscript;
using org.kevoree.modeling.api.json;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Microkernel;
using System;
using System.Text;

namespace Org.Kevoree.Core.Bootstrap
{
    public class Bootstrap
    {
        private readonly static JSONModelLoader jsonLoader;
        private readonly KevoreeCoreBean core = new KevoreeCoreBean();
        private KevScriptEngine kevScriptEngine = new KevScriptEngine();
        private KevoreeKernel microkernel;
        public KevoreeCLKernel kernel;


        public Bootstrap(KevoreeKernel k, string nodeName, string kevoreeRegistryUrl, string nugetLocalRepositoryPath, string nugetRepositoryUrl)
        {
            //xmiLoader = core.getFactory().createXMILoader();
            this.microkernel = k;
            this.kernel = new KevoreeCLKernel(this, nodeName, nugetLocalRepositoryPath, nugetRepositoryUrl);


            /* WARNING : Really hacky*/
            java.lang.System.setProperty("kevoree.registry", kevoreeRegistryUrl);

            this.core.setNodeName(nodeName);
            //this.kernel.setNodeName(nodeName);
            this.kernel.setCore(this.core);
            core.setBootstrapService(kernel);
            this.core.start();
        }

        static void Main(string[] args)
        {
            var options = new CommandLineOptions();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {

                try
                {
                    var boot = new Bootstrap(KevoreeKernel.Self.Value, options.NodeName, options.KevoreeRegistryUrl, options.NugetLocalRepositoryPath, options.NugetRepositoryUrl);
                    if (options.ScriptPath == null)
                    {
                        /*
                         * A default model is loaded.
                         * It is composed of a WSGroup and a Dotnet Node, attached together.
                         * The Dotnet Node name is "nodeName".
                         * The WSGroup listen on port 9000.
                         */
                        string defaultKevScript = String.Format(@"add {0} : DotnetNode
                                                            add sync : WSGroup
                                                            attach {0} sync", options.NodeName);
                        boot.loadKevScript(defaultKevScript, (x) => System.Console.WriteLine("Bootstrap completed"));
                    }
                    else
                    {
                        /*
                         * We try to load scriptPath file and guess it's content by extention (.json for a json model or .kev for a kev script model).
                         */
                        boot.loadScript(options.ScriptPath);
                    }
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }
            }
        }

        private void loadKevScript(string defaultKevScript, UpdateCallback callback)
        {
            ContainerRoot emptyModel = initialModel();
            core.getFactory().root(emptyModel);
            java.io.InputStream input = new ByteArrayInputStream(Encoding.Unicode.GetBytes(defaultKevScript.ToCharArray()));
            System.Console.WriteLine(defaultKevScript);
            try
            {
                kevScriptEngine.execute(defaultKevScript, emptyModel);
            }
            catch (java.lang.Exception e)
            {
                //System.Console.WriteLine(e.getMessage());
                throw new System.Exception(e.getMessage());

            }

            ContainerNode currentNode = emptyModel.findNodesByID(core.getNodeName());

            // TODO : is it really necessary to deal with network issues here ?

            core.update(emptyModel, callback, "/");

            //System.Console.WriteLine("Done");

        }

        private void loadScript(string scriptPath)
        {
            if (scriptPath.EndsWith(".json"))
            {
                // TODO : load a json model
                var model = jsonLoader.loadModelFromStream(new FileInputStream(scriptPath)).get(0);
                bootstrap((ContainerRoot)model, (applied) => System.Console.WriteLine(applied));
            }
            else if (scriptPath.EndsWith(".kev"))
            {
                // TODO : load a kev script
            }
            else
            {
                // TODO : allowing XMI ?
                System.Console.WriteLine(String.Format("Unknow file extension [{0}]", scriptPath));
            }
        }

        private void bootstrap(ContainerRoot model, UpdateCallback callback)
        {
            ContainerRoot emptyModel = initialModel();
            core.getFactory().root(emptyModel);
            org.kevoree.pmodeling.api.compare.ModelCompare compare = core.getFactory().createModelCompare();
            compare.merge(emptyModel, model).applyOn(emptyModel);
            core.update(emptyModel, callback, "/");
        }

        private ContainerRoot initialModel()
        {
            ContainerRoot emptyModel = this.core.getFactory().createContainerRoot();
            return emptyModel;
        }
    }
}
