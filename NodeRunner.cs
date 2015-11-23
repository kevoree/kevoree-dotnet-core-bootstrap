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
using System.Collections.Generic;
using System.IO;

namespace Org.Kevoree.Core.Bootstrap
{
    public class NodeRunner : MarshalByRefObject, INodeRunner
    {
        private readonly AnnotationHelper annotationHelpler = new AnnotationHelper();
        private readonly KevoreeInjector<KevoreeInject> injector = new KevoreeInjector<KevoreeInject>();
        private CompositionContainer _container;
        [ImportMany(typeof(Org.Kevoree.Annotation.DeployUnit))]
        private HashSet<Org.Kevoree.Annotation.DeployUnit> exports;
        private string pluginPath;
        private string packageName;
        private string packageVersion;
        private readonly KevoreeInjector<Param> injectorParams = new KevoreeInjector<Param>();
        private DeployUnit node;

        public void setPluginPath(string pluginPath)
        {
            this.pluginPath = pluginPath;
        }

        public void setPackageName(string packageName)
        {
            this.packageName = packageName;
        }

        public void setPackageVersion(string packageVersion)
        {
            this.packageVersion = packageVersion;
        }

        public AdaptationModel plan(IContainerRootMarshalled current, IContainerRootMarshalled target,
            ITracesSequence sequence )
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
            var targetPath = Path.Combine(this.pluginPath, packageName + "." + packageVersion);
            var plugDir = new FileInfo(targetPath).Directory;
            var catalogs = plugDir.GetDirectories("*", SearchOption.AllDirectories).Select(x => new DirectoryCatalog(x.FullName));
            var directoryAggregate = new AggregateCatalog(catalogs);
            _container = new CompositionContainer(directoryAggregate);
            _container.ComposeParts(this);
            this.node = exports.First();
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