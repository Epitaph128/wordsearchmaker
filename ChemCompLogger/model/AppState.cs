using ChemCompLogger.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemCompLogger.model
{
    class AppState
    {
        private DbState dbState;
        private MenuState menuState;
        private int uniquePuzzles = 1;
        private bool allowReverseWords = false;
        private int overlapPercentage = 50;
        private Tuple<int, int> puzzleDimensions;
        private List<Tuple<int, int>> allowablePuzzleDimensions;
        private int puzzleDimensionIndex;

        internal AppState(AppConsole appConsole)
        {
            dbState = new DbState();

            menuState = new MenuState(appConsole);

            puzzleDimensions = new Tuple<int, int>(24, 16);
            puzzleDimensionIndex = 4;

            allowablePuzzleDimensions = new List<Tuple<int, int>>();
            allowablePuzzleDimensions.Add(new Tuple<int, int>(12, 12));
            allowablePuzzleDimensions.Add(new Tuple<int, int>(16, 12));
            allowablePuzzleDimensions.Add(new Tuple<int, int>(12, 16));
            allowablePuzzleDimensions.Add(new Tuple<int, int>(16, 16));
            allowablePuzzleDimensions.Add(new Tuple<int, int>(24, 16));
            allowablePuzzleDimensions.Add(new Tuple<int, int>(16, 24));
            allowablePuzzleDimensions.Add(new Tuple<int, int>(24, 24));
        }

        internal DbState GetDbState()
        {
            return dbState;
        }

        internal MenuState GetMenuState()
        {
            return menuState;
        }

        internal int UniquePuzzles
        {
            get
            {
                return uniquePuzzles;
            }
            set
            {
                uniquePuzzles = value;
            }
        }

        internal bool AllowReverseWords
        {
            get
            {
                return allowReverseWords;
            }
            set
            {
                allowReverseWords = value;
            }
        }

        internal int OverlapPercentage
        {
            get
            {
                return overlapPercentage;
            }
            set
            {
                overlapPercentage = value;
            }
        }

        internal Tuple<int, int> PuzzleDimensions
        {
            get
            {
                return puzzleDimensions;
            }
            set
            {
                puzzleDimensions = value;
            }
        }

        internal void TogglePuzzleDimensions(bool reverseOrder)
        {
            if (reverseOrder)
            {
                puzzleDimensionIndex--;
                if (puzzleDimensionIndex == -1)
                {
                    puzzleDimensionIndex = allowablePuzzleDimensions.Count - 1;
                }
            } else
            {
                puzzleDimensionIndex++;
                if (puzzleDimensionIndex == allowablePuzzleDimensions.Count)
                {
                    puzzleDimensionIndex = 0;
                }
            }
            PuzzleDimensions = allowablePuzzleDimensions[puzzleDimensionIndex];
        }
    }
}
