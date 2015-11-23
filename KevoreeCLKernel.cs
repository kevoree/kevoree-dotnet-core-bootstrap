using System;
using System.Linq;
using org.kevoree;
using org.kevoree.impl;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.IMarshalled;
using Boolean = java.lang.Boolean;

namespace Org.Kevoree.Core.Bootstrap
{
    public class KevoreeCLKernel : MarshalByRefObject, BootstrapService
    {
        private readonly string nodeName;
        private readonly string nugetLocalRepositoryPath;
        private readonly string nugetRepositoryUrl;
        private Bootstrap bootstrap;
        private KevoreeCoreBean core;

        public KevoreeCLKernel(Bootstrap bootstrap, string nodeName, string nugetLocalRepositoryPath,
            string nugetRepositoryUrl)
        {
            this.bootstrap = bootstrap;
            this.nodeName = nodeName;
            this.nugetLocalRepositoryPath = nugetLocalRepositoryPath;
            this.nugetRepositoryUrl = nugetRepositoryUrl;
        }

        /**
         * DEVNOTE : on pose comme prédicat que cette méthode est dédiée à la création d'instance de NODE et rien d'autre
         */

        public INodeRunner createInstance(IContainerNodeMarshalled nodeInstance)
        {
            var typedef = nodeInstance.getTypeDefinition();
            // FIXME : look badly complex for just a DU look (we are looking for the DU of dotnet).
            var deployUnitDotNet =
                ((IDeployUnitMarshalled)
                    typedef.getDeployUnits()
                        .Where(x => x.findFiltersByID("platform").getValue() == "dotnet")
                        .First());
            var name = deployUnitDotNet.getName();
            var version = deployUnitDotNet.getVersion();
            var instance = new NugetLoader.NugetLoader(nugetLocalRepositoryPath).LoadRunnerFromPackage<NodeRunner>(name, version, nugetRepositoryUrl);
            // TODO : ici injecter les @KevoreeInject dans l'instance
            //var coreProxy = new ContextAwareModelServiceCoreProxy();
            instance.proceedInject(nodeInstance.path(), nodeName, nodeInstance.getName(), core);


            return instance;
        }

        public IComponentRunner LoadSomething(string name, string version, string path)
        {
            ComponentRunner ret = new NugetLoader.NugetLoader(nugetLocalRepositoryPath).LoadRunnerFromPackage<ComponentRunner>(
                       name, version, nugetRepositoryUrl);
            ret.ProceedInject(path, nodeName, name, core);
            return ret;
        }


        public void setCore(KevoreeCoreBean core)
        {
            this.core = core;
        }

    }
}