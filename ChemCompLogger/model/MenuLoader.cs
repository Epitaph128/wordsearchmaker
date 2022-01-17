using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using ChemCompLogger.view;

/* This class handles loading a YAML based document
 * in order to produce an interactive story in WPF (using an 8 button grid panel, and a simple textual console display). Uses https://github.com/aaubry/YamlDotNet to handle YAML processing.
*/

namespace ChemCompLogger.model
{
    public class MenuLoader
    {
        // menu loader spaces in tab quantity for tab to space replacement before parsing
        private const int SPACES_IN_TAB = 4;

        static public Menu LoadMenuFromString(String data)
        {
            var input = new StringReader(data);

            // setup deserializer engine
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            // enable this to convert tabs to spaces
            input = TabToSpaceBeforeLoad(input, SPACES_IN_TAB);

            var menu = deserializer.Deserialize<Menu>(input);

            if (menu.Pages.Select(x => x.PageName).Distinct().Count() != menu.Pages.Count)
            {
                throw new Exception("menus.yaml contains two pages with the same pageName.");
            }

            return menu;
        }

        // load a story given a file name
        static public Menu LoadMenu(String fileName)
        {
            // load in the story program (YAML document) to input string
            var doc = File.ReadAllText(fileName);
            var menu = LoadMenuFromString(doc);
            return menu;
        }

        private static StringReader TabToSpaceBeforeLoad(StringReader input, int spacesInTab)
        {
            // deserialize input stream as page objects
            var inputString = input.ReadToEnd();

            // setup tab replace w/ spaces before deserialization
            string s = "";
            for (int i = 0; i < spacesInTab; i++)
            {
                s += " ";
            }

            var yamlTabSafety = inputString.Replace("\t", s);
            return new StringReader(yamlTabSafety);
        }
    }
}
