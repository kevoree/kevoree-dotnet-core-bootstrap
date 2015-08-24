using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Kevoree.Core.Bootstrap
{
    class Program
    {
        static void Main(string[] args)
        {
            string nodeName = "node0";
            string scriptPath = null;
            bool showHelp = false;

            var optionSet = new OptionSet()
            {
                { "node.name=", "the main node name (default: node0)", n => nodeName = n },
                { "node.script=", "init script path", p => scriptPath = p },
                { "h|help=", "displays help message", v => showHelp = true }
            };

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("kevoree-dotnet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `kevoree-dotnet --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }
        }

        private static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Usage: kevoree-dotnet [OPTIONS]+");
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
