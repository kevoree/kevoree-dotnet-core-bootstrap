using NUnit.Framework;
using System;

namespace Org.Kevoree.Core.Bootstrap
{

    // TODO : RM ME
	[TestFixture ()]
	public class NugetClassLoaderTest
	{
		[Test ()]
		public void TestCase ()
		{
			new NugetClassLoader ().Load ("org-kevoree-yield-version", "3.0.0");
		}
	}
}

