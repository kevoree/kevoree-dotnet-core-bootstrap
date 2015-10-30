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
        protected readonly AnnotationHelper annotationHelpler = new AnnotationHelper();
        protected readonly KevoreeInjector<KevoreeInject> injector = new KevoreeInjector<KevoreeInject>();
        protected readonly KevoreeInjector<Param> injectorParams = new KevoreeInjector<Param>();
        protected readonly KevoreeInjector<Output> injectorOutputs = new KevoreeInjector<Output>();
        protected readonly KevoreeInjector<Input> injectorInputs = new KevoreeInjector<Input>();
        protected readonly KevoreeInjector<Dispatch> injectorDispatchs = new KevoreeInjector<Dispatch>();
        private CompositionContainer container;
        private DirectoryCatalog directoryCatalog;
        protected DeployUnit component;
        private string pluginPath;

        public void setPluginPath(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        private void Start()
        {
            var startsMethods = annotationHelpler.filterMethodsByAttribute(component.GetType(), typeof(Start));
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
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(NodeRunner).Assembly, regBuilder));
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
            var startMethods = annotationHelpler.filterMethodsByAttribute(component.GetType(), typeof(Stop));
            if (startMethods.Count() > 0)
            {
                var startMethod = startMethods.First();
                startMethod.Invoke(component, null);
            }
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

        public void attachOutputPort(Port port, string fieldName)
        {
            injectorOutputs.injectByName(component, port, fieldName);
        }

        public void sendThroughInputPort(string fieldName, string value)
        {
            injectorInputs.callByName(component, fieldName, value);
        }

        public void dispatch(string payload, Callback callback)
        {
            injectorDispatchs.call(component, payload);
        }

        public void attachInputPort(Port port)
        {
            // component must be a channel
            ((ChannelPort)component).addInputPort(port);
        }

        public void attachRemoteInputPort(Port port)
        {
            // component must be a channel
            ((ChannelPort)component).addRemoteInputPort(port);
        }

        public void detachInputPort(Port port)
        {
            ((ChannelPort)component).removeInputPort(port);
        }

        public void detachRemoteInputPort(Port port)
        {
            ((ChannelPort)component).removeRemoteInputPort(port);
        }
    }
}