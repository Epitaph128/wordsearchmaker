using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChemCompLogger.view
{
    class AppConsole
    {
        private System.Windows.Controls.TextBox textBox;

        private static FileVersionInfo fv = System.Diagnostics.FileVersionInfo.GetVersionInfo
                               (Assembly.GetExecutingAssembly().Location);

        internal static string wordSearchMakerVersion = "0.2";

        private string welcomeText = @"
  -------------------------------
        :--:--:--:--:--:--:
           :--:--:--:--:
             :--:-:--:
                | |
         Word Search Maker
                | |
        :--:--:--:--:--:--:

        V {0}

        :--:--:--:--:--:--:
               :-:-:
                :-:
                 |
    This utility is designed to allow
    a user to create simple word
    search puzzles.

    Hotkeys: F1-F4 toggle preset window layouts.
    Escape exits the utility from non-table editing views.

    Creating a puzzle requires a modern web-browser installed
    for viewing / printing.

    The binary path for this utility must be user-writable for
    the database.

    Hold Shift before adding unique puzzles to add 10 at a time.

    Hold Shift when adjusting puzzle dimensions to move in reverse
    through the allowable dimension configurations.

    When Creating Puzzle w/ the generator, the answers and the unsolved
    puzzles will be displayed in two separate tabs of the browser (with
    the unsolved appearing after.)

    Built using (w/o official permission but using free software):
    - C#/XAML/WPF
        - https://docs.microsoft.com/en-us/dotnet/framework/wpf/
    - YamlDotNet by Antoine Aubry
        - https://aaubry.net/pages/yamldotnet.html
    - Bootstrap for Report Generator output styling:
        - https://github.com/twbs/bootstrap

    Created by Walter Macfarland
                |
            ---:-:
            /
    ----:-:
    /
:-:
";

        // constructor
        public AppConsole(ref System.Windows.Controls.TextBox textBox)
        {
            var versionSplitByPeriods = wordSearchMakerVersion.Split(new char[] { '.' });
            Int32 majorVersionVal;
            var majorVersion = Int32.TryParse((System.String)versionSplitByPeriods.GetValue(0), out majorVersionVal);
            if (majorVersionVal < 1)
            {
                Int32 minorVersionVal;
                var minorVersion = Int32.TryParse((System.String)versionSplitByPeriods.GetValue(1), out minorVersionVal);
                if (minorVersionVal > 1)
                {
                    wordSearchMakerVersion += " (BETA)";
                }
                else
                {
                    wordSearchMakerVersion += " (ALPHA)";
                }
            }
            this.textBox = textBox;
            Write(string.Format(welcomeText, wordSearchMakerVersion));
        }

        public void Write(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            this.textBox.AppendText(s.Trim() + Environment.NewLine + Environment.NewLine);
            this.textBox.ScrollToEnd();
        }

        public void AddLineBreak()
        {
            this.textBox.AppendText(Environment.NewLine);
            this.textBox.ScrollToEnd();
        }
    }
}
