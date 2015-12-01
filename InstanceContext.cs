using Org.Kevoree.Core.Api;

namespace Org.Kevoree.Core.Bootstrap
{
    internal class InstanceContext : Context
    {
        private readonly string _instanceName;
        private readonly string _nodeName;
        private readonly string _path;

        public InstanceContext(string path, string nodeName, string instanceName)
        {
            _path = path;
            _nodeName = nodeName;
            _instanceName = instanceName;
        }

        public string getPath()
        {
            return _path;
        }

        public string getNodeName()
        {
            return _nodeName;
        }

        public string getInstanceName()
        {
            return _instanceName;
        }
    }
}