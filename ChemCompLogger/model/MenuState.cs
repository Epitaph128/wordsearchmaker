using ChemCompLogger.view;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChemCompLogger.model
{
    class MenuState
    {
        private Menu menu;

        private Dictionary<string, MenuPage> pages;

        private string previousPage;
        private string currentPage;

        private AppConsole appConsole;

        internal MenuState(AppConsole ac)
        {
            appConsole = ac;

            var menuData = Encoding.UTF8.GetString(ChemCompLogger.Properties.Resources.menus);
            menu = MenuLoader.LoadMenuFromString(menuData);

            pages = new Dictionary<string, MenuPage>();
            foreach (var page in menu.Pages)
            {
                pages.Add(page.PageName, page);
                pages[page.PageName].Visited = false;
            }

            // set currentPage to smallest pageNo in story
            previousPage = "";
            MenuPage menuPage;
            pages.TryGetValue("main_menu", out menuPage);
            if (menuPage != null)
            {
                currentPage = menuPage.PageName;
            }
        }

        // page accessor methods
        internal void ShowOpeningText()
        {
            if (pages[currentPage].Visited && pages[currentPage].TextReturn != null)
            {
                appConsole.Write(pages[currentPage].TextReturn);
            }
            else
            {
                pages[currentPage].Visited = true;
                appConsole.Write(pages[currentPage].TextOpen);
            }
        }

        internal void ShowClosingText()
        {
            var s = (pages[currentPage].TextClose == null) ?
                "" : pages[currentPage].TextClose;
            appConsole.Write(s);
        }

        internal string GetCurrentPage()
        {
            return currentPage;
        }

        internal string GetPreviousPage()
        {
            return previousPage;
        }

        // currentPage accessor methods
        internal void SetCurrentPage(string pageName)
        {
            if (pages.ContainsKey(pageName))
            {
                ShowClosingText();
                previousPage = currentPage;
                currentPage = pageName;
                ShowOpeningText();
            }
            else
            {
                appConsole.Write($"Error: page does not exist in story: {pageName}");
            }
        }

        internal string[] GetButtonDisplays()
        {
            string[] buttonDisplays = new string[8];

            foreach (MenuButton sb in pages[currentPage].Buttons)
            {
                buttonDisplays[sb.Num] = sb.Text;
            }

            return buttonDisplays;
        }

        internal string ButtonClicked(int buttonNumber)
        {
            var button = pages[currentPage].Buttons.Find(i => i.Num == buttonNumber);
            if (button != null)
            {
                // set initial content to button click text if available
                string content = button.TextClick == null ?
                    "" :
                    button.TextClick;

                appConsole.Write(content);

                if (!string.IsNullOrWhiteSpace(button.Action))
                {
                    return "";
                }
                else
                {
                    SetCurrentPage(button.Go);
                    return button.Go;
                }
            }
            else
            {
                appConsole.Write($"Error: Button does not exist with num: {buttonNumber}");
                return "";
            }
        }

        internal string ButtonAction(int buttonNumber)
        {
            var button = pages[currentPage].Buttons.Find(i => i.Num == buttonNumber);
            if (button != null)
            {
                if (button.Action != null)
                {
                    return button.Action;
                }
            }
            return "";
        }

        internal MenuButton GetButton(int buttonNumber)
        {
            var button = pages[currentPage].Buttons.Find(i => i.Num == buttonNumber);
            return button;
        }
    }
}