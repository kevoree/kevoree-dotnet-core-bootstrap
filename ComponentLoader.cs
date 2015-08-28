using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Kevoree.Core.Bootstrap
{
    class ComponentLoader
    {
        public static Runner loadComponent(string packageName, string packageVersion)
        {
            var cachePath = DirectoryNameManager.getShadowCopyPath(packageName, packageVersion);
            var pluginPath = DirectoryNameManager.getPluginPath(packageName, packageVersion);
            return LoadPlugin(cachePath, pluginPath);
        }

        private static Runner LoadPlugin(string cachePath, string pluginPath)
        {
            // This creates a ShadowCopy of the MEF DLL's 
            // (and any other DLL's in the ShadowCopyDirectories)
            var setup = new AppDomainSetup
            {
                CachePath = pluginPath,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = cachePath
            };

            // Create a new AppDomain then create a new instance 
            // of this application in the new AppDomain.            
            return NewAppDomain(setup);
        }

        private static Runner NewAppDomain(AppDomainSetup setup)
        {
            AppDomain domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), AppDomain.CurrentDomain.Evidence, setup);
            var runner = (Runner)domain.CreateInstanceAndUnwrap(typeof(Runner).Assembly.FullName, typeof(Runner).FullName);

            runner.setDomain(domain);

            return runner;
        }
    }
}
