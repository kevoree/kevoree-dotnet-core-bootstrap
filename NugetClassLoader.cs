using System;
using NuGet;
using System.Reflection;
using System.IO;

namespace Org.Kevoree.Core.Bootstrap
{
	public class NugetClassLoader
	{
		public NugetClassLoader ()
		{
		}

		public void Load (string packageName, string version)
		{
			
			var repo = PackageRepositoryFactory.Default.CreateRepository ("https://packages.nuget.org/api/v2");
            string finalPath = "Plugins";
            string path = GetPath(packageName, version, finalPath);
			var packageManager = new PackageManager (repo, path);

			var AppDomainToolkitPackage = repo.FindPackage ("AppDomainToolkit", SemanticVersion.Parse ("1.0.4.3"));
			packageManager.InstallPackage (AppDomainToolkitPackage, false, true);
			
			var package = repo.FindPackage (packageName, SemanticVersion.Parse (version));

			//var domain = AppDomain.CreateDomain (Guid.NewGuid ().ToString ());

			if (package != null) {
				packageManager.InstallPackage (package, false, true);
			}

		}

        public static string GetPath(string packageName, string version, string finalPath)
        {
            string rootPath = @"C:\Users\mleduc\Desktop\NUGET";
            string path = Path.Combine(rootPath, packageName, version, finalPath);
            return path;
        }

		/*private static void PackageManager_PackageInstalled(object sender, PackageOperationEventArgs e)
		{
			var files = e.FileSystem.GetFiles(e.InstallPath, "*.dll", true);
			foreach (var file in files)
			{
				try
				{
					AppDomain domain = AppDomain.CreateDomain("tmp");
					Type typeProxyType = typeof(TypeProxy);
					var typeProxyInstance = (TypeProxy)domain.CreateInstanceAndUnwrap(
						typeProxyType.Assembly.FullName,
						typeProxyType.FullName);

					var type = typeProxyInstance.LoadFromAssembly(file, "<KnownTypeName>");
					object instance = domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
				}
				catch (Exception ex)
				{
					Console.WriteLine("failed to load {0}", file);
					Console.WriteLine(ex.ToString());
				}

			}
		}*/
	}
}

