using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Kevoree.Core.Bootstrap
{
    class DirectoryNameManager
    {
        public static string getShadowCopyPath(string packageName, string packageVersion)
        {
            return getFullPath(packageName, packageVersion, "ShadowCopyCache");
        }

        public static string getPluginPath(string packageName, string packageVersion)
        {
            return getFullPath(packageName, packageVersion, "Plugins");
        }

        private static string getFullPath(string packageName, string version, string finalPath)
        {
            // TODO : magic path to replace
            string rootPath = @"C:\Users\mleduc\Desktop\NUGET";
            var path = Path.Combine(rootPath, packageName, version, finalPath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}
