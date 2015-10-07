using System;
using org.kevoree;
using org.kevoree.pmodeling.api.trace;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Handler;

namespace Org.Kevoree.Core.Bootstrap
{
    internal class ContextAwareAdapter : ModelService
    {
        private readonly string caller;
        private readonly ContextAwareModelService service;

        public ContextAwareAdapter(ContextAwareModelService core, string path)
        {
            service = core;
            caller = path;
        }

        public UUIDModel getCurrentModel()
        {
            return service.getCurrentModel();
        }

        public ContainerRoot getPendingModel()
        {
            return service.getPendingModel();
        }

        public void compareAndSwap(ContainerRoot model, Guid uuid, UpdateCallback callback)
        {
            service.compareAndSwap(model, uuid, callback, caller);
        }

        public void update(ContainerRoot model, UpdateCallback callback)
        {
            service.update(model, callback, caller);
        }

        public void registerModelListener(ModelListener listener)
        {
            service.registerModelListener(listener, caller);
        }

        public void unregisterModelListener(ModelListener listener)
        {
            service.unregisterModelListener(listener, caller);
        }

        public void acquireLock(LockCallBack callBack, long timeout)
        {
            service.acquireLock(callBack, timeout, caller);
        }

        public void releaseLock(Guid uuid)
        {
            service.releaseLock(uuid, caller);
        }

        public string getNodeName()
        {
            return service.getNodeName();
        }


        public void submitScript(string script, UpdateCallback callback)
        {
            service.submitScript(script, callback, caller);
        }

        public void submitSequence(TraceSequence sequence, UpdateCallback callback)
        {
            service.submitSequence(sequence, callback, caller);
        }
    }
}