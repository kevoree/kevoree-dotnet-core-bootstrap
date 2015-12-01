using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Org.Kevoree.Annotation;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.IMarshalled;
using Org.Kevoree.Library.Annotation;

namespace Org.Kevoree.Core.Bootstrap
{
    public class ComponentRunner : MarshalByRefObject, IComponentRunner
    {

        protected readonly AnnotationHelper AnnotationHelpler = new AnnotationHelper();
        protected readonly KevoreeInjector<KevoreeInject> Injector = new KevoreeInjector<KevoreeInject>();
        protected readonly KevoreeInjector<Param> InjectorParams = new KevoreeInjector<Param>();
        protected readonly KevoreeInjector<Output> InjectorOutputs = new KevoreeInjector<Output>();
        protected readonly KevoreeInjector<Input> InjectorInputs = new KevoreeInjector<Input>();
        protected readonly KevoreeInjector<Dispatch> InjectorDispatchs = new KevoreeInjector<Dispatch>();
        private CompositionContainer _container;
        private DeployUnit _component;

        [ImportMany(typeof(DeployUnit))]
        private HashSet<DeployUnit> _exports;

        private string _pluginPath;
        private string _packageName;
        private string _packageVersion;

        public void setPluginPath(string pluginPath)
        {
            _pluginPath = pluginPath;
        }

        public void setPackageName(string packageName)
        {
            _packageName = packageName;
        }

        public void setPackageVersion(string packageVersion)
        {
            _packageVersion = packageVersion;
        }

        private void Start()
        {
            var startsMethods = AnnotationHelpler.filterMethodsByAttribute(_component.GetType(), typeof(Start));
            if (startsMethods.Any())
            {
                var startMethod = startsMethods.First();
                startMethod.Invoke(_component, null);
            }
        }

        private void Init()
        {
            var targetPath = Path.Combine(_pluginPath, _packageName + "." + _packageVersion);
            var plugDir = new FileInfo(targetPath).Directory;
            var catalogs = plugDir.GetDirectories("*", SearchOption.AllDirectories).Select(x => new DirectoryCatalog(x.FullName));
            var directoryAggregate = new AggregateCatalog(catalogs);
            _container = new CompositionContainer(directoryAggregate);
            _container.ComposeParts(this);
            _component = _exports.First();
        }

        public bool Run()
        {
            Start();
            return true;
        }

        public bool Stop()
        {
            var startMethods = AnnotationHelpler.filterMethodsByAttribute(_component.GetType(), typeof(Stop));
            if (startMethods.Any())
            {
                var startMethod = startMethods.First();
                startMethod.Invoke(_component, null);
            }
            return true;
        }

        internal void ProceedInject(string path, string nodeName, string name, KevoreeCoreBean core)
        {
            Init();
            Injector.inject<Context>(_component, new InstanceContext(path, nodeName, name));
            Injector.inject<ModelService>(_component, new ContextAwareAdapter(core, path));
            Injector.inject(_component, core.getLogger().getInstance(nodeName, name));
            Injector.inject(_component, core.getBootstrapService());
        }

        public bool updateDictionary(IDictionaryAttributeMarshalled attribute, IValueMarshalled value)
        {
            InjectorParams.smartInject<Param>(_component, attribute.getName(), attribute.getDatatype(), value.getValue());
            return true;
        }

        public void attachOutputPort(Port port, string fieldName)
        {
            InjectorOutputs.injectByName(_component, port, fieldName);
        }

        public void sendThroughInputPort(string fieldName, string value)
        {
            InjectorInputs.callByName(_component, fieldName, value);
        }

        public void dispatch(string payload, Callback callback)
        {
            InjectorDispatchs.call(_component, payload);
        }

        public void attachInputPort(Port port)
        {
            // component must be a channel
            (_component as ChannelPort).addInputPort(port);
        }

        public void attachRemoteInputPort(Port port)
        {
            // component must be a channel
            (_component as ChannelPort).addRemoteInputPort(port);
        }

        public void detachInputPort(Port port)
        {
            (_component as ChannelPort).removeInputPort(port);
        }

        public void detachRemoteInputPort(Port port)
        {
            (_component as ChannelPort).removeRemoteInputPort(port);
        }
    }
}