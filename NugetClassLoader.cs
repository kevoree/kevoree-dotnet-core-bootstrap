using System;
using NuGet;
using System.Reflection;
using System.IO;
using AppDomainToolkit;

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
			string path = "/tmp/kevoreenuget";
			var packageManager = new PackageManager (repo, path);

			var AppDomainToolkitPackage = repo.FindPackage ("AppDomainToolkit", SemanticVersion.Parse ("1.0.4.3"));
			packageManager.InstallPackage (AppDomainToolkitPackage, false, true);
			
			var package = repo.FindPackage (packageName, SemanticVersion.Parse (version));

			//var domain = AppDomain.CreateDomain (Guid.NewGuid ().ToString ());

			if (package != null) {
				packageManager.InstallPackage (package, false, true);
				foreach (PhysicalPackageFile iPackageAssemblyReference in package.AssemblyReferences) {
					//Assembly.LoadFile (iPackageAssemblyReference.SourcePath);
					/*Type typeProxyType = typeof(TypeProxy);

					//var type = typeProxyInstance.LoadFromAssembly (iPackageAssemblyReference.SourcePath, "Org.Kevoree.YieldVersion.YieldVersion");
					object instance = domain.CreateInstanceAndUnwrap (iPackageAssemblyReference.SourcePath, "Org.Kevoree.YieldVersion.YieldVersion");
					Console.WriteLine ("yes ?." + instance.ToString ());*/
					var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					var setupInfo = new AppDomainSetup()
					{
						//ApplicationName = "My Application",
						ApplicationBase = path,
						PrivateBinPath = path 
					};
					using(var context = AppDomainContext.Create(setupInfo)) {
						// chemin dans tmp : iPackageAssemblyReference.SourcePath
						context.LoadAssemblyWithReferences (LoadMethod.LoadFile, path + "/" + packageName + "." + version + "/" + "lib/net40/YieldVersion.dll");
						RemoteAction.Invoke (context.Domain, () => {
							var msg = " from AppDomain " + AppDomain.CurrentDomain.CreateInstanceAndUnwrap("YieldVersion", "Org.Kevoree.YieldVersion.YieldVersion");
							//var b = a.GetType("Org.Kevoree.YieldVersion.YieldVersion");
							Console.WriteLine(msg);
						});
						Console.WriteLine ("okok");
					}
				}
			}

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

