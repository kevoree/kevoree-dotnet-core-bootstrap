using System.Threading;

namespace Org.Kevoree.Core.Microkernel
{
    public abstract class KevoreeKernel
    {
        public readonly static ThreadLocal<KevoreeKernel> Self = new ThreadLocal<KevoreeKernel>();
    }
}
