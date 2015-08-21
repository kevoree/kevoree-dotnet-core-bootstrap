using NUnit.Framework;
using System;
using ConsoleApplication1;

namespace Org.Kevoree.Core.Bootstrap
{
	[TestFixture ()]
	public class SeparateAppDomainAssemblyLoaderTest
	{
		[Test ()]
		public void TestCase ()
		{

			Console.WriteLine (typeof(SeperateAppDomainAssemblyLoader).ToString());
			var type = Type.GetType ("ConsoleApplication.SeperateAppDomainAssemblyLoader, SeperateAppDomainAssemblyLoader");
			Activator.CreateInstance (type);
			System.IO.FileInfo param = new System.IO.FileInfo ("/tmp/kevoreenuget/org-kevoree-yield-version.3.0.0/lib/net40/YieldVersion.dll");
			var result = new SeperateAppDomainAssemblyLoader ().LoadAssembly(param);
			Console.WriteLine (result);
		}
	}
}

