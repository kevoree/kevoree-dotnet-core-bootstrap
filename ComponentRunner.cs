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
using NodeType = Org.Kevoree.Core.Api.NodeType;
using Org.Kevoree.Log.Api;

namespace Org.Kevoree.Core.Bootstrap
{
    public class ComponentRunner : MarshalByRefObject, IComponentRunner
    {
        private readonly AnnotationHelper annotationHelpler = new AnnotationHelper();
        private readonly KevoreeInjector<KevoreeInject> injector = new KevoreeInjector<KevoreeInject>();
        private readonly KevoreeInjector<Param> injectorParams = new KevoreeInjector<Param>();
        private readonly KevoreeInjector<Output> injectorOutputs = new KevoreeInjector<Output>();
        private readonly KevoreeInjector<Input> injectorInputs = new KevoreeInjector<Input>();
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;
        private DeployUnit component;
        private string pluginPath;

        public void setPluginPath(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        private void Start()
        {
            var startsMethods = annotationHelpler.filterMethodsByAttribute(component.GetType(), typeof (Start));
            if (startsMethods.Count() > 0)
            {
                var startMethod = startsMethods.First();
                startMethod.Invoke(component, null);
            }
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
            component = laliste.First();
        }

        public bool Run()
        {
            Start();
            return true;
        }

        public bool Stop()
        {
            var startMethod = annotationHelpler.filterMethodsByAttribute(component.GetType(), typeof(Stop)).First();
            startMethod.Invoke(component, null);
            return true;
        }

        internal void ProceedInject(string path, string nodeName, string name, KevoreeCoreBean core)
        {
            Init();
            injector.inject<Context>(component, new InstanceContext(path, nodeName, name));
            injector.inject<ModelService>(component, new ContextAwareAdapter(core, path));
            injector.inject<ILogger>(component, core.getLogger().getInstance(nodeName, name));
            injector.inject(component, core.getBootstrapService());
        }

        public bool updateDictionary(IDictionaryAttributeMarshalled attribute, IValueMarshalled value)
        {
            injectorParams.smartInject<Param>(component, attribute.getName(), attribute.getDatatype(), value.getValue());
            return true;
        }

        public void attacheOutputPort(Port port, string fieldName)
        {
            injectorOutputs.injectByName(component, port, fieldName);
        }

        public void sendThroughInputPort(string value, string fieldName)
        {
            injectorInputs.callByName(component, fieldName, value);
        }
    }
}