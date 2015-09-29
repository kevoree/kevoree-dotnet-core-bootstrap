using Org.Kevoree.Annotation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using Org.Kevoree.Core.Api;

namespace Org.Kevoree.Core.Bootstrap
{

    public class Runner : MarshalByRefObject, IRunner
    {
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;
        private IEnumerable<DeployUnit> exports;
        private AppDomain domain;
        private string pluginPath;

        public void setPluginPath(string pluginPath) {
            this.pluginPath = pluginPath;
        }
    }

}