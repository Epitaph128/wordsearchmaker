using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemCompLogger.model
{
    /* These classes define the hierarchy built by the StoryLoader which
     * represents the story structure. */
    public class Menu
    {
        public List<MenuPage> Pages { get; set; }
        public List<MenuArt> Art { get; set; }
    }

    public class MenuPage
    {
        public string PageName { get; set; }
        public string TextOpen { get; set; }
        public string TextClose { get; set; }
        public string TextReturn { get; set; }
        public bool Visited { get; set; }
        public List<MenuButton> Buttons { get; set; }
    }

    public class MenuButton
    {
        public int Num { get; set; }
        public string Action { get; set; }
        public string Go { get; set; }
        public string Text { get; set; }
        public string TextClick { get; set; }
    }

    public class MenuArt
    {
        public int ArtNo { get; set; }
        public string Text { get; set; }
    }
}
