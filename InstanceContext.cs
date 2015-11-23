using Org.Kevoree.Core.Api;

namespace Org.Kevoree.Core.Bootstrap
{
    internal class InstanceContext : Context
    {
        private readonly string instanceName;
        private readonly string nodeName;
        private readonly string path;

        public InstanceContext(string path, string nodeName, string instanceName)
        {
            this.path = path;
            this.nodeName = nodeName;
            this.instanceName = instanceName;
        }

        public string getPath()
        {
            return path;
        }

        public string getNodeName()
        {
            return nodeName;
        }

        public string getInstanceName()
        {
            return instanceName;
        }
    }
}