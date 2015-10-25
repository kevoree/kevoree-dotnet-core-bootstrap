using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using Org.Kevoree.Annotation;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Adaptation;
using Org.Kevoree.Core.Api.Command;
using Org.Kevoree.Core.Api.IMarshalled;
using Org.Kevoree.Library.Annotation;
using Org.Kevoree.Log.Api;
using NodeType = Org.Kevoree.Core.Api.NodeType;

namespace Org.Kevoree.Core.Bootstrap
{
    public class NodeRunner : MarshalByRefObject, INodeRunner
    {
        private readonly AnnotationHelper annotationHelpler = new AnnotationHelper();
        private readonly KevoreeInjector<KevoreeInject> injector = new KevoreeInjector<KevoreeInject>();
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;
        private DeployUnit node;
        private string pluginPath;
        private readonly KevoreeInjector<Param> injectorParams = new KevoreeInjector<Param>();

        public void setPluginPath(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        public AdaptationModel plan(IContainerRootMarshalled current, IContainerRootMarshalled target,
            ITracesSequence sequence)
        {
            return ((NodeType) node).plan(current, target, sequence);
        }

        public ICommand getPrimitive(AdaptationPrimitive primitive)
        {
            return ((NodeType) node).getPrimitive(primitive);
        }

        public void Start()
        {
            //this.Init();
            var startMethod = annotationHelpler.filterMethodsByAttribute(node.GetType(), typeof (Start)).First();
            startMethod.Invoke(node, null);
        }

        private void Init()
        {
            // Use RegistrationBuilder to set up our MEF parts.
            var regBuilder = new RegistrationBuilder();
            regBuilder.ForTypesDerivedFrom<DeployUnit>().Export<DeployUnit>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (NodeRunner).Assembly, regBuilder));
            directoryCatalog = new DirectoryCatalog(pluginPath, regBuilder);
            catalog.Catalogs.Add(directoryCatalog);

            container = new CompositionContainer(catalog);
            container.ComposeExportedValue(container);

            // Get our exports available to the rest of Program.
            var laliste = container.GetExportedValues<DeployUnit>();
            node = laliste.First();
        }

        public void proceedInject(string path, string nodeName, string name, KevoreeCoreBean core)
        {
            Init();
            injector.inject<Context>(node, new InstanceContext(path, nodeName, name));
            injector.inject<ModelService>(node, new ContextAwareAdapter(core, path));
            injector.inject<ILogger>(node, core.getLogger().getInstance(name));
            injector.inject(node, core.getBootstrapService());
        }

        public bool updateDictionary(IDictionaryAttributeMarshalled attribute, IValueMarshalled value)
        {
            injectorParams.smartInject<Param>(node, attribute.getName(), attribute.getDatatype(), value.getValue());
            return true;
        }
    }
}