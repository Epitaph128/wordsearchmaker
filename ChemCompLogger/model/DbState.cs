using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChemCompLogger.model
{
    class DbState
    {
        private readonly string filename = @".\wordbank.data";

        private DbState_WordBank wordBank;
        private FileStream databaseFileStream;
        private PuzzleGenerator puzzleGenerator;

        public DbState()
        {
            wordBank = new DbState_WordBank(this);

            puzzleGenerator = new PuzzleGenerator(wordBank);
        }

        public DbState_WordBank WordBank
        {
            get
            {
                return wordBank;
            }
        }

        public FileStream GetDatabase()
        {
            return databaseFileStream;
        }

        internal void InitializeDatabase()
        {
            // create db file if non-existent
            if (!File.Exists(filename))
            {
                databaseFileStream = File.Create(filename);
            }
            else
            {
                databaseFileStream = new FileStream(filename, FileMode.OpenOrCreate);
            }
        }

        internal void CreateNewDatabase()
        {
            databaseFileStream.Close();
            databaseFileStream = File.Create(filename);
        }

        internal void CreatePuzzle(AppState appState, String puzzleTitle)
        {
            puzzleGenerator.CreatePuzzles(appState, puzzleTitle);
        }
    }
}
