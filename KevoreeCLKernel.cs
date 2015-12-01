using System;
using System.Linq;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.IMarshalled;

namespace Org.Kevoree.Core.Bootstrap
{
    public class KevoreeCLKernel : MarshalByRefObject, BootstrapService
    {
        private readonly string _nodeName;
        private readonly string _nugetLocalRepositoryPath;
        private readonly string _nugetRepositoryUrl;
        private KevoreeCoreBean _core;

        public KevoreeCLKernel(string nodeName, string nugetLocalRepositoryPath,
            string nugetRepositoryUrl)
        {
            _nodeName = nodeName;
            _nugetLocalRepositoryPath = nugetLocalRepositoryPath;
            _nugetRepositoryUrl = nugetRepositoryUrl;
        }

        /**
         * DEVNOTE : on pose comme prédicat que cette méthode est dédiée à la création d'instance de NODE et rien d'autre
         */

        public INodeRunner createInstance(IContainerNodeMarshalled nodeInstance)
        {
            var typedef = nodeInstance.getTypeDefinition();
            var deployUnitDotNet = typedef.getDeployUnits().First(x => x.findFiltersByID("platform").getValue() == "dotnet");
            var name = deployUnitDotNet.getName();
            var version = deployUnitDotNet.getVersion();
            var instance = new NugetLoader.NugetLoader(_nugetLocalRepositoryPath).LoadRunnerFromPackage<NodeRunner>(name, version, _nugetRepositoryUrl);
            instance.ProceedInject(nodeInstance.path(), _nodeName, nodeInstance.getName(), _core);

            return instance;
        }

        public IComponentRunner LoadSomething(string name, string version, string path)
        {
            ComponentRunner ret = new NugetLoader.NugetLoader(_nugetLocalRepositoryPath).LoadRunnerFromPackage<ComponentRunner>(
                       name, version, _nugetRepositoryUrl);
            ret.ProceedInject(path, _nodeName, name, _core);
            return ret;
        }


        public void SetCore(KevoreeCoreBean core)
        {
            _core = core;
        }

    }
}