using org.kevoree;
using org.kevoree.api;
using Org.Kevoree.Annotation;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Adaptation;
using Org.Kevoree.Library.Annotation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;

namespace Org.Kevoree.Core.Bootstrap
{

    public class NodeRunner : MarshalByRefObject, INodeRunner
    {
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;
        Org.Kevoree.Annotation.DeployUnit node;
        private string pluginPath;
        private readonly AnnotationHelper annotationHelpler = new AnnotationHelper();
        private KevoreeInjector<KevoreeInject> injector = new KevoreeInjector<KevoreeInject>();

        public void setPluginPath(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        private void Init()
        {
            // Use RegistrationBuilder to set up our MEF parts.
            var regBuilder = new RegistrationBuilder();
            regBuilder.ForTypesDerivedFrom<Org.Kevoree.Annotation.DeployUnit>().Export<Org.Kevoree.Annotation.DeployUnit>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(NodeRunner).Assembly, regBuilder));
            directoryCatalog = new DirectoryCatalog(pluginPath, regBuilder);
            catalog.Catalogs.Add(directoryCatalog);

            container = new CompositionContainer(catalog);
            container.ComposeExportedValue(container);

            // Get our exports available to the rest of Program.
            var laliste = container.GetExportedValues<Org.Kevoree.Annotation.DeployUnit>();
            node = laliste.First();
        }

        public AdaptationModel plan(ContainerRoot actualModel, ContainerRoot targetModel)
        {
            // TODO
            return null;
        }

        public Org.Kevoree.Core.Api.PrimitiveCommand getPrimitive(AdaptationPrimitive primitive)
        {
            // TODO
            return null;
        }

        public void Start()
        {
            //this.Init();
            var startMethod = this.annotationHelpler.filterMethodsByAttribute(node.GetType(), typeof(Start)).First();
            startMethod.Invoke(node, null);
        }

        public void proceedInject(string path, string nodeName, string name, KevoreeCoreBean core)
        {
            this.Init();
            this.injector.inject<Context>(node, new InstanceContext(path, nodeName, name));
            this.injector.inject<Org.Kevoree.Core.Api.ModelService>(node, new ContextAwareAdapter(core, path));
            this.injector.inject<Org.Kevoree.Core.Api.BootstrapService>(node, core.getBootstrapService());
        }
    }

}