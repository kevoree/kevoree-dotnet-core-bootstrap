using System;
using Org.Kevoree.Core.Api;
using Org.Kevoree.Core.Api.Handler;
using Org.Kevoree.Core.Api.IMarshalled;

namespace Org.Kevoree.Core.Bootstrap
{
    internal class ContextAwareAdapter : ModelService
    { 
        private readonly string _caller;
        private readonly ContextAwareModelService _service;

        public ContextAwareAdapter(ContextAwareModelService core, string path)
        {
            _service = core;
            _caller = path;
        }

        public UUIDModel getCurrentModel()
        {
            return _service.getCurrentModel();
        }

        public IContainerRootMarshalled getPendingModel()
        {
            return _service.getPendingModel();
        }

        public void compareAndSwap(IContainerRootMarshalled model, Guid uuid, UpdateCallback callback)
        {
            _service.compareAndSwap(model, uuid, callback, _caller);
        }

        public void update(IContainerRootMarshalled model, UpdateCallback callback)
        {
            _service.update(model, callback, _caller);
        }

        public void registerModelListener(ModelListener listener)
        {
            _service.registerModelListener(listener, _caller);
        }

        public void unregisterModelListener(ModelListener listener)
        {
            _service.unregisterModelListener(listener, _caller);
        }

        public void acquireLock(LockCallBack callBack, long timeout)
        {
            _service.acquireLock(callBack, timeout, _caller);
        }

        public void releaseLock(Guid uuid)
        {
            _service.releaseLock(uuid, _caller);
        }

        public string getNodeName()
        {
            return _service.getNodeName();
        }

		public void submitSequence(org.kevoree.modeling.api.trace.TraceSequence sequence, Org.Kevoree.Core.Api.UpdateCallback callback)
		{

			 _service.submitSequence(sequence, callback, _caller);
		}

        public void submitScript(string script, UpdateCallback callback)
        {
            _service.submitScript(script, callback, _caller);
        }

     
    }
}