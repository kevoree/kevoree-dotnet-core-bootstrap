using org.kevoree;
using org.kevoree.modeling.api.trace;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Handler;
using Org.Kevoree.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Kevoree.Core.Bootstrap
{
    class ContextAwareAdapter: Org.Kevoree.Core.Api.ModelService
    {
        private ContextAwareModelService service;
        private string caller;

        public ContextAwareAdapter(ContextAwareModelService core, string path)
        {
            this.service = core;
            this.caller = path;
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


        public void submitScript(string script, Org.Kevoree.Core.Api.UpdateCallback callback)
        {
            service.submitScript(script, callback, caller);
        }

        public void submitSequence(org.kevoree.pmodeling.api.trace.TraceSequence sequence, UpdateCallback callback)
        {
            service.submitSequence(sequence, callback, caller);
        }
    }
}
