using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Org.Kevoree.Annotation;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Adaptation;
using Org.Kevoree.Core.Api.Command;
using Org.Kevoree.Core.Api.IMarshalled;
using Org.Kevoree.Library.Annotation;
using NodeType = Org.Kevoree.Core.Api.NodeType;

namespace Org.Kevoree.Core.Bootstrap
{
    public class NodeRunner : MarshalByRefObject, INodeRunner
    {
        private readonly AnnotationHelper _annotationHelpler = new AnnotationHelper();
        private readonly KevoreeInjector<KevoreeInject> _injector = new KevoreeInjector<KevoreeInject>();
        private CompositionContainer _container;
        [ImportMany(typeof(DeployUnit))]
        private HashSet<DeployUnit> _exports;
        private string _pluginPath;
        private string _packageName;
        private string _packageVersion;
        private readonly KevoreeInjector<Param> _injectorParams = new KevoreeInjector<Param>();
        private DeployUnit _node;

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

        public AdaptationModel plan(IContainerRootMarshalled current, IContainerRootMarshalled target,
            ITracesSequence sequence )
        {
            return (_node as NodeType).plan(current, target, sequence);
        }

        public ICommand getPrimitive(AdaptationPrimitive primitive)
        {
            return (_node as NodeType).getPrimitive(primitive);
        }

        public void Start()
        {
            var startMethod = _annotationHelpler.filterMethodsByAttribute(_node.GetType(), typeof (Start)).First();
            startMethod.Invoke(_node, null);
        }

        private void Init()
        {
            var targetPath = Path.Combine(_pluginPath, _packageName + "." + _packageVersion);
            var plugDir = new FileInfo(targetPath).Directory;
            var catalogs = plugDir.GetDirectories("*", SearchOption.AllDirectories).Select(x => new DirectoryCatalog(x.FullName));
            var directoryAggregate = new AggregateCatalog(catalogs);
            _container = new CompositionContainer(directoryAggregate);
            _container.ComposeParts(this);
            _node = _exports.First();
        }

        public void ProceedInject(string path, string nodeName, string name, KevoreeCoreBean core)
        {
            Init();
            _injector.inject<Context>(_node, new InstanceContext(path, nodeName, name));
            _injector.inject<ModelService>(_node, new ContextAwareAdapter(core, path));
            _injector.inject(_node, core.getLogger().getInstance(name));
            _injector.inject(_node, core.getBootstrapService());
        }

        public bool updateDictionary(IDictionaryAttributeMarshalled attribute, IValueMarshalled value)
        {
            _injectorParams.smartInject<Param>(_node, attribute.getName(), attribute.getDatatype(), value.getValue());
            return true;
        }
    }
}