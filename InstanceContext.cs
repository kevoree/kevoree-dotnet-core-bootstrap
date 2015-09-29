using Org.Kevoree.Core.Api;

namespace Org.Kevoree.Core.Bootstrap
{
    class InstanceContext : Context
    {
        private readonly string path;
        private readonly string nodeName;
        private readonly string instanceName;

        public InstanceContext(string path, string nodeName, string instanceName)
        {
            this.path = path;
            this.nodeName = nodeName;
            this.instanceName = instanceName;
        }

        public string getPath() { return this.path; }

        public string getNodeName() { return this.nodeName; }

        public string getInstanceName() { return this.instanceName; }
    }
}
