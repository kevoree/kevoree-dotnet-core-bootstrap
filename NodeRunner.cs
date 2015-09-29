using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Adaptation;
using org.kevoree;

namespace Org.Kevoree.Core.Bootstrap
{

    public class NodeRunner : MarshalByRefObject, INodeRunner
    {
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;
        private IEnumerable<org.kevoree.DeployUnit> exports;
        private AppDomain domain;
        private string pluginPath;

        public void setPluginPath(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        public AdaptationModel plan(ContainerRoot actualModel, ContainerRoot targetModel)
        {
            // TODO
            return null;
        }

        public PrimitiveCommand getPrimitive(AdaptationPrimitive primitive)
        {
            // TODO
            return null;
        }

        public void Start()
        {

        }
    }

}