using System;
using NuGet;
using System.Reflection;
using System.IO;

namespace Org.Kevoree.Core.Bootstrap
{
    // TODO : RM ME
	public class NugetClassLoader
	{
		public NugetClassLoader ()
		{
		}

		public void Load (string packageName, string version)
		{
			
			var repo = PackageRepositoryFactory.Default.CreateRepository ("https://packages.nuget.org/api/v2");
            string finalPath = "Plugins";
            //string path = GetPath(packageName, version, finalPath);
			//var packageManager = new PackageManager (repo, path);

			var AppDomainToolkitPackage = repo.FindPackage ("AppDomainToolkit", SemanticVersion.Parse ("1.0.4.3"));
			//packageManager.InstallPackage (AppDomainToolkitPackage, false, true);
			
			var package = repo.FindPackage (packageName, SemanticVersion.Parse (version));

			if (package != null) {
				//packageManager.InstallPackage (package, false, true);
			}

		}


	}
}

