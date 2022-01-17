using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ChemCompLogger.model
{
    internal class PuzzleGenerator
    {
        private DbState_WordBank wordBank;

        private int puzDimX = 24;
        private int puzDimY = 16;

        private bool printPuzzleSolution = true;
        private Random randomGen;

        private const int const_wordPlacementAttempts = 320000;
        private const int const_wordOverlapAttempts = 5000;
        private const int const_wordOverlapShuffles = 20;
        private const int const_puzzleGenerationAttemptsForOverlapMaximization = 25;

        private List<string> filesToOpen;

        public PuzzleGenerator(DbState_WordBank wordBank)
        {
            this.wordBank = wordBank;
            filesToOpen = new List<string>();
        }

        public class Puzzle
        {
            public char[] board;
            public int[] boardPlacements;
            public int overlaps;  
            public Puzzle(int puzDimX, int puzDimY)
            {
                board = new char[puzDimX * puzDimY];
                boardPlacements = new int[puzDimX * puzDimY];
                overlaps = 0;
            }
        }
        internal String GetStringFromCharArray(char[] chars)
        {
            String s = "";
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == 0)
                {
                    break;
                }
                s += chars[i];
            }
            return s;
        }

        internal int GetBoardPos(int x, int y)
        {
            return y * puzDimX + x;
        }

        internal int CheckChar(char[] board, char c, int x, int y)
        {
            if (x > puzDimX - 1 || y > puzDimY - 1 || x < 0 || y < 0)
            {
                return 3;
            }
            if (board[GetBoardPos(x, y)] == 0)
            {
                return 0;
            }
            if (board[GetBoardPos(x, y)] == c)
            {
                return 1;
            }
            return 2;
        }

        // return true is overwriting a different character
        internal Tuple<bool, int> PlaceChar(char[] board, int[] boardPlacements, int overlaps,
            char c, int x, int y)
        {
            if (board[GetBoardPos(x, y)] == 0)
            {
                board[GetBoardPos(x, y)] = c;
                boardPlacements[GetBoardPos(x, y)]++;
                return new Tuple<bool, int>(true, overlaps);
            }
            else if (board[GetBoardPos(x, y)] == c)
            {
                boardPlacements[GetBoardPos(x, y)]++;
                overlaps++;
                return new Tuple<bool, int>(true, overlaps);
            }
            return new Tuple<bool, int>(false, overlaps);
        }

        internal Puzzle GeneratePuzzle(AppState appState)
        {
            var boardSize = puzDimX * puzDimY;
            var board = new char[boardSize];
            var boardPlacements = new int[boardSize];

            for (int i = 0; i < boardSize; i++)
            {
                board[i] = (char)0;
                boardPlacements[i] = 0;
            }

            int overlaps = 0;

            var charPlacements = new List<Tuple<int, int>>[26];
            for (int i = 0; i < 26; i++)
            {
                charPlacements[i] = new List<Tuple<int, int>>();
            }

            // place all words in puzzle
            for (int i = 0; i < wordBank.Words.Count(); i++)
            {
                var word = wordBank.Words[i].Text.ToUpper().Replace(" ", "");
                var wordIsReversed = appState.AllowReverseWords && (int)(randomGen.NextDouble() * 100) > 75;
                if (wordIsReversed)
                {
                    var wordReversed = GetStringFromCharArray(word.Reverse().ToArray());
                }
                var attempts = 0;

                // random sequence determined for overlapping
                List<int> positionSequence = new List<int>();
                for (int c = 0; c < word.Length; c++)
                {
                    positionSequence.Add(c);
                }
                var wordOverlapShuffles = appState.OverlapPercentage == 100 ?
                    const_wordOverlapShuffles * 100 : const_wordOverlapShuffles;
                for (int n = 0; n < wordOverlapShuffles; n++)
                {
                    var p1 = (int)(randomGen.NextDouble() * word.Length);
                    var p2 = (int)(randomGen.NextDouble() * word.Length);
                    if (p1 != p2)
                    {
                        var t = positionSequence[p1];
                        positionSequence[p1] = positionSequence[p2];
                        positionSequence[p2] = t;
                    }
                }

                while (true)
                {
                    var wordPlacementAttempts = appState.OverlapPercentage == 100 ?
                        const_wordPlacementAttempts * 10 : const_wordPlacementAttempts;
                    if (attempts > wordPlacementAttempts)
                    {
                        return null;
                    }
                    int x = -1;
                    int y = -1;
                    var dir = (int)(randomGen.NextDouble() * 4);
                    if (wordIsReversed)
                    {
                        dir += 4;
                    }
                    var wordCheckFailed = false;

                    var overlapSkipValue = (int)(randomGen.NextDouble() * 100);
                    var overlapWordAttempts = appState.OverlapPercentage == 100 ?
                        const_wordOverlapAttempts * 100 : const_wordOverlapAttempts;
                    // should choose chars at random for better results
                    if (overlapSkipValue <= appState.OverlapPercentage && attempts < overlapWordAttempts)
                    {
                        var charToMatchWithPos = attempts % word.Length;
                        var matchesForChar = charPlacements[word[positionSequence[charToMatchWithPos]] - 'A'].Count();
                        if (matchesForChar > 0)
                        {
                            var charToAttemptOverlapWith =
                                charPlacements[word[positionSequence[charToMatchWithPos]] - 'A'][(int)(randomGen.NextDouble() * matchesForChar)];
                            x = charToAttemptOverlapWith.Item1;
                            y = charToAttemptOverlapWith.Item2;
                            switch (dir)
                            {
                                case 0: // up right
                                    x -= positionSequence[charToMatchWithPos];
                                    y += positionSequence[charToMatchWithPos];
                                    break;
                                case 1: // right
                                    x -= positionSequence[charToMatchWithPos];
                                    break;
                                case 2: // down right
                                    x -= positionSequence[charToMatchWithPos];
                                    y -= positionSequence[charToMatchWithPos];
                                    break;
                                case 3: // down
                                    y -= positionSequence[charToMatchWithPos];
                                    break;
                                case 4: // down left
                                    x += positionSequence[charToMatchWithPos];
                                    y -= positionSequence[charToMatchWithPos];
                                    break;
                                case 5: // left
                                    x += positionSequence[charToMatchWithPos];
                                    break;
                                case 6: // up left
                                    x += positionSequence[charToMatchWithPos];
                                    y += positionSequence[charToMatchWithPos];
                                    break;
                                case 7: // up
                                    y += positionSequence[charToMatchWithPos];
                                    break;
                            }
                        }
                        else
                        {
                            attempts++;
                            continue;
                        }
                    }

                    // if no position set by overlapping calc,
                    // then give random x/y
                    if (x == -1 || y == -1)
                    {
                        x = (int)(randomGen.NextDouble() * puzDimX);
                        y = (int)(randomGen.NextDouble() * puzDimY);
                    }

                    // check if word can place
                    for (int c = 0; c < word.Length; c++)
                    {
                        var r = CheckChar(board, word[c], x, y);
                        if (r >= 2)
                        {
                            wordCheckFailed = true;
                            break;
                        }
                        switch (dir)
                        {
                            case 0: // up right
                                x += 1;
                                y -= 1;
                                break;
                            case 1: // right
                                x += 1;
                                break;
                            case 2: // down right
                                x += 1;
                                y += 1;
                                break;
                            case 3: // down
                                y += 1;
                                break;
                            case 4: // down left
                                x -= 1;
                                y += 1;
                                break;
                            case 5: // left
                                x -= 1;
                                break;
                            case 6: // up left
                                x -= 1;
                                y -= 1;
                                break;
                            case 7: // up
                                y -= 1;
                                break;
                        }
                    }

                    if (wordCheckFailed)
                    {
                        attempts++;
                        continue;
                    }

                    for (int c = word.Length - 1; c >= 0; c--)
                    {
                        switch (dir)
                        {
                            case 0: // up right
                                x -= 1;
                                y += 1;
                                break;
                            case 1: // right
                                x -= 1;
                                break;
                            case 2: // down right
                                x -= 1;
                                y -= 1;
                                break;
                            case 3: // down
                                y -= 1;
                                break;
                            case 4: // down left
                                x += 1;
                                y -= 1;
                                break;
                            case 5: // left
                                x += 1;
                                break;
                            case 6: // up left
                                x += 1;
                                y += 1;
                                break;
                            case 7: // up
                                y += 1;
                                break;
                        }
                        var r = PlaceChar(board, boardPlacements, overlaps, word[c], x, y);
                        if (!r.Item1)
                        {
                            throw new Exception("Attempted to place character in invalid location.");
                        }
                        overlaps = r.Item2;
                        // indicate where char is placed for matching
                        charPlacements[word[c] - 'A'].Add(new Tuple<int, int>(x, y));
                    }
                    break;
                }
            }

            // fill remaining squares with random letters
            var letters = new char[]{ 'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
                'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

            for (int i = 0; i < boardSize; i++)
            {
                if (board[i] == 0)
                {
                    var randomLetter = (int)(randomGen.NextDouble() * (Double)letters.Length);
                    board[i] = letters[randomLetter];
                }
            }

            var foundWords = new List<string>();
            var directionsToCheck = appState.AllowReverseWords ? 8 : 4;

            // check all words are in puzzle
            for (int i = 0; i < wordBank.Words.Count(); i++)
            {
                var word = wordBank.Words[i].Text.ToUpper().Replace(" ", "");
                bool wordFound = false;
                int foundAtX = 0;
                int foundAtY = 0;
                for (int x = 0; x < puzDimX; x++)
                {
                    for (int y = 0; y < puzDimY; y++)
                    {
                        if (board[GetBoardPos(x, y)] == word[0])
                        {
                            if (word.Length == 1)
                            {
                                wordFound = true;
                                break;
                            }
                            foundAtX = x;
                            foundAtY = y;
                            // check in each valid direction for sequential characters
                            for (int dir = 0; dir < directionsToCheck; dir++)
                            {
                                int x2 = foundAtX;
                                int y2 = foundAtY;
                                int cOffset = 1;
                                while (cOffset < word.Length)
                                {
                                    switch (dir)
                                    {
                                        case 0: // up right
                                            x2 += 1;
                                            y2 -= 1;
                                            break;
                                        case 1: // right
                                            x2 += 1;
                                            break;
                                        case 2: // down right
                                            x2 += 1;
                                            y2 += 1;
                                            break;
                                        case 3: // down
                                            y2 += 1;
                                            break;
                                        case 4: // down left
                                            x2 -= 1;
                                            y2 += 1;
                                            break;
                                        case 5: // left
                                            x2 -= 1;
                                            break;
                                        case 6: // up left
                                            x2 -= 1;
                                            y2 -= 1;
                                            break;
                                        case 7: // up
                                            y2 -= 1;
                                            break;
                                    }
                                    if (CheckChar(board, word[cOffset], x2, y2) != 1)
                                    {
                                        break;
                                    }
                                    cOffset++;
                                    if (cOffset == word.Length)
                                    {
                                        wordFound = true;
                                    }
                                }
                                if (wordFound) break;
                            }
                        }
                        if (wordFound) break;
                    } // end for y
                    if (wordFound) break;
                } // end for x
                if (!wordFound)
                {
                    throw new Exception("Puzzle generated but word missing: " + word);
                }
                else
                {
                    foundWords.Add(word + " starts at (" + foundAtX + ", " + foundAtY + ")");
                }
            }

            Puzzle p = new Puzzle(puzDimX, puzDimY);
            p.board = board;
            p.boardPlacements = boardPlacements;
            p.overlaps = overlaps;
            return p;
        }

        internal void WriteStringToStream(Stream stream, String text)
        {
            foreach (char c in text)
            {
                stream.WriteByte((byte)c);
            }
        }

        internal void PrintPuzzles(List<Puzzle> puzzles, bool debug, String puzzleTitle)
        {
            printPuzzleSolution = debug;
            Directory.CreateDirectory("Puzzles");
            var dateStamp = System.DateTime.UtcNow.ToShortDateString() + "_" + System.DateTime.UtcNow.ToShortTimeString() + "-" + System.DateTime.UtcNow.Second;
            dateStamp = dateStamp.Replace("/", "-");
            dateStamp = dateStamp.Replace(":", "-");
            dateStamp = dateStamp.Replace(" ", "");
            FileStream fs;
            if (debug)
            {
                fs = File.Create("./Puzzles/puzzle_key_" + dateStamp + ".html");
            }
            else
            {
                fs = File.Create("./Puzzles/puzzle_" + dateStamp + ".html");
            }

            var pathToExe = Properties.Resources.bootstrap_min;

            Stream contentStream = new MemoryStream(128000000);

            WriteStringToStream(contentStream,
                @"<!DOCTYPE html><html><head><style>" + pathToExe + "</style></head><br/>");

            //var bootstrap_style = "<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css\" integrity=\"sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T\" crossorigin=\"anonymous\">";

            var custom_style = @"
table
{
    text-align: center;
    margin: 1em auto;
}
.table-puzzle
{
    border: 2px solid black;
}
.partOfWord
{
    color: #FFAA00;
}
.partOfMWords
{
    color: red;
}
.credits
{
    text-align: center;
    font-size: 0.7em;
}
h1, h2, h3, h4, h5, h6
{
    text-align: center;
}";

            var normalPuzzleLetterStyle = @"
.table-puzzle td
{
    font-size: 2em;
    padding: 0 8px;
}";

            var smallPuzzleLetterStyle = @"
.table-puzzle td
{
    font-size: 1.5em;
    padding: 0 8px;
}";

            var normalWordListStyle = @"
.word-list td
{
    font-size: 1.5em;
    padding-left: 64px;
    padding-right: 64px;
}";

            var smallWordListStyle = @"
.word-list td
{
    font-size: 1em;
    padding-left: 64px;
    padding-right: 64px;
}";

            if (puzDimY == 24)
            {
                custom_style += smallPuzzleLetterStyle;
            }
            else
            {
                custom_style += normalPuzzleLetterStyle;
            }

            if (puzDimY < 24 || wordBank.Words.Count <= 32)
            {
                custom_style += normalWordListStyle;
            }
            else
            {
                custom_style += smallWordListStyle;
            }

            for (int i = 0; i < puzzles.Count(); i++)
            {
                WriteStringToStream(contentStream,
                    "<h3>" + puzzleTitle + "</h3><br/>");

                WriteStringToStream(contentStream,
                    "<table class=\"table-sm table-bordered table-puzzle\" style=\"margin: auto\">");
                // print puzzle out
                for (int y = 0; y < puzDimY; y++)
                {
                    WriteStringToStream(contentStream, "\n<tr>");
                    for (int x = 0; x < puzDimX; x++)
                    {
                        if (printPuzzleSolution && puzzles[i].boardPlacements[GetBoardPos(x, y)] == 1)
                        {
                            WriteStringToStream(contentStream, "<td class='partOfWord'>" +
                                puzzles[i].board[GetBoardPos(x, y)] + "</td>");
                        }
                        else if (printPuzzleSolution && puzzles[i].boardPlacements[GetBoardPos(x, y)] > 1)
                        {
                            WriteStringToStream(contentStream, "<td class='partOfMWords'>" +
                                puzzles[i].board[GetBoardPos(x, y)] + "</td>");
                        }
                        else
                        {
                            WriteStringToStream(contentStream, "<td>" + puzzles[i].board[GetBoardPos(x, y)] +
                                "</td>");
                        }
                    }
                    WriteStringToStream(contentStream, "</tr>");
                }
                WriteStringToStream(contentStream, "</table>\n<table class='word-list'>");

                var wy = 4;
                var wx = (int)Math.Ceiling(wordBank.Words.Count() / (Double)wy);

                for (int x = 0; x < wx; x++)
                {
                    WriteStringToStream(contentStream, "<tr>");
                    for (int y = 0; y < wy; y++)
                    {
                        if (y * wx + x < wordBank.Words.Count())
                        {
                            WriteStringToStream(contentStream, "<td>" + wordBank.Words[y * wx + x].Text + "</td>");
                        }
                    }
                    WriteStringToStream(contentStream, "</tr>");
                }
                WriteStringToStream(contentStream, "</table>\n");

                // make sure content fills page
                WriteStringToStream(contentStream, "<p style=\"page-break-before: always\"/>\n");
            }

            WriteStringToStream(contentStream, string.Format("\n<style>{0}</style>", custom_style) +
                "</html>");

            // reset stream to head
            contentStream.Seek(0, SeekOrigin.Begin);
            do
            {
                var b = contentStream.ReadByte();
                if (b == -1)
                {
                    break;
                }
                fs.WriteByte((byte)b);
            } while (true);

            fs.Close();

            // queue file to be opened after puzzle generation completes
            filesToOpen.Add(fs.Name);

            contentStream.Close();
        }

        internal void CreatePuzzles(AppState appState, String puzzleTitle)
        {
            randomGen = new Random();
            // setting puzzle dimensions this way prohibits multi-threaded
            // puzzle generation (TODO if performance is issue.)
            puzDimX = appState.PuzzleDimensions.Item1;
            puzDimY = appState.PuzzleDimensions.Item2;

            List<Puzzle> bestPuzzles = new List<Puzzle>();

            for (int i = 0; i < appState.UniquePuzzles; i++)
            {
                var puzzle = GeneratePuzzle(appState);
                if (puzzle == null)
                {
                    MessageBox.Show("Could not place all words in puzzle. Try shorter or fewer words.", "Error", MessageBoxButton.OK);
                    return;
                }
                bestPuzzles.Add(puzzle);
            }

            PrintPuzzles(bestPuzzles, true, puzzleTitle);
            PrintPuzzles(bestPuzzles, false, puzzleTitle);
            foreach (String s in filesToOpen)
            {
                System.Diagnostics.Process.Start(s);
            }
            filesToOpen.Clear();
        }
    }
}
