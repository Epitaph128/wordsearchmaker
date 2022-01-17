using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace ChemCompLogger.model
{
    class DbState_WordBank
    {
        private long WordSearchMakerVersion = 1;
        private ObservableCollection<Word> wordBankCollection;
        private List<long> deletedWordIDs;
        private bool wordBankCollectionCollectionModifiedViaDeletion = false;
        private long latestwordBankID;
        private DbState dbState;

        public DbState_WordBank(DbState dbState)
        {
            deletedWordIDs = new List<long>();
            this.dbState = dbState;
        }

        internal void DeleteWord(IList source)
        {
            foreach (var deletedObj in source)
            {
                var deletedWordID = deletedObj as Word;
                if (deletedWordID != null)
                {
                    deletedWordIDs.Add(deletedWordID.ID);
                }
            }
            if (deletedWordIDs.Any())
            {
                foreach (long id in deletedWordIDs)
                {
                    var matchingItem = wordBankCollection.Where(x => x.ID == id).FirstOrDefault();
                    if (matchingItem != null)
                    {
                        wordBankCollection.Remove(matchingItem);
                        wordBankCollectionCollectionModifiedViaDeletion = true;
                    }
                }
            }
        }

        public bool WordBankCollectionModifiedViaDeletion
        {
            get
            {
                return wordBankCollectionCollectionModifiedViaDeletion;
            }
            private set
            {
                wordBankCollectionCollectionModifiedViaDeletion = value;
            }
        }

        public ObservableCollection<Word> Words
        {
            get
            {
                return wordBankCollection;
            }
            private set
            {
                wordBankCollection = value;
            }
        }

        public int ValidWordCount
        {
            get
            {
                int validWordCount = 0;
                foreach (Word w in wordBankCollection)
                {
                    if (w.TextIsValid)
                    {
                        validWordCount++;
                    }
                }

                return validWordCount;
            }
        }

        internal bool CheckModified_WordBank()
        {
            if (wordBankCollection == null)
            {
                return false;
            }
            return wordBankCollection.Any(x => x.Modified) || wordBankCollectionCollectionModifiedViaDeletion;
        }

        internal String GetStringFromBytes(byte[] bytes)
        {
            String s = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                {
                    break;
                }
                s += (char)bytes[i];
            }
            return s;
        }

        internal Tuple<byte[], int> ReadField(byte[] dbBytes, int offset, int maxBytes)
        {
            byte[] returnBytes = new byte[maxBytes];
            int newOffset = offset;
            for (int i = 0; i < maxBytes; i++)
            {
                if (i >= dbBytes.Length)
                {
                    break;
                }
                if (dbBytes[i + offset] == '\n')
                {
                    newOffset++;
                    break;
                }
                returnBytes[i] = dbBytes[offset + i];
                newOffset++;
            }
            return new Tuple<byte[], int>(returnBytes, newOffset);
        }

        internal ObservableCollection<Word> Query_Subjective_Analyses()
        {
            dbState.InitializeDatabase();
            dbState.GetDatabase().Seek(0, System.IO.SeekOrigin.Begin);
            deletedWordIDs.Clear();
            wordBankCollectionCollectionModifiedViaDeletion = false;
            wordBankCollection = new ObservableCollection<Word>();
            latestwordBankID = 0;

            var dbBytes = new byte[dbState.GetDatabase().Length + 1];
            dbBytes[dbState.GetDatabase().Length] = (byte)'\n';
            // assuming there will only be max int chunks within long of intMax-bytes per chunk
            int chunksToRead = (int)(dbState.GetDatabase().Length / int.MaxValue) + 1;
            int remainder = (int)(dbState.GetDatabase().Length % int.MaxValue);

            for (int i = 0; i < chunksToRead; i += int.MaxValue)
            {
                var bytesToRead = (i == chunksToRead - 1) ? remainder : int.MaxValue;
                var partialBytes = new byte[bytesToRead];

                // note: length read is int, but length of FileStream is long
                // this means that if the length of file is > int max the extra
                // bytes will be dropped from reading. NEED TO FIX WHEN INTERNET IS POSSIBLE
                object readToken = (object)25;
                var result = dbState.GetDatabase().BeginRead(partialBytes,
                    (chunksToRead - 1) * int.MaxValue, bytesToRead, null, readToken);
                dbState.GetDatabase().EndRead(result);

                // copy bytes from byte[intMax] container to the byte[longMax] container
                for (int x = 0; x < bytesToRead; x++)
                {
                    dbBytes[(chunksToRead - 1) * int.MaxValue + x] = partialBytes[x];
                }
            }

            // read records into subjectiveAnalysesCollection

            var int64MaxChars = 20;
            var userTextMaxChars = 6400;
            //byte[] id = new byte[int64MaxChars];
            byte[] version = new byte[int64MaxChars];
            long dataVersion = 0;

            var c = 0;

            var int64Tuple = ReadField(dbBytes, c, int64MaxChars);
            System.Int64 versionValue;
            System.String versionValueString = GetStringFromBytes(int64Tuple.Item1);
            System.Int64.TryParse(versionValueString, out versionValue);
            c = int64Tuple.Item2;
            dataVersion = versionValue;

            byte[] wordText = new byte[userTextMaxChars];

            while (c < dbState.GetDatabase().Length)
            {
                var word = new Word();

                var userStringTuple = ReadField(dbBytes, c, userTextMaxChars);
                word.Text = GetStringFromBytes(userStringTuple.Item1);
                c = userStringTuple.Item2;

                word.ID = ++latestwordBankID;

                // needs to be last property set (changing others trigger notify property change event)
                word.Modified = false;
                wordBankCollection.Add(word);
            }
            dbState.GetDatabase().Close();
            return wordBankCollection;
        }

        internal void ClearEmptyWords()
        {
            var newWords = new ObservableCollection<Word>();
            foreach (Word w in Words)
            {
                if (w.Text == null || w.Text.Length == 0)
                {
                    continue;
                }
                newWords.Add(w);
            }
            Words = newWords;
        }

        internal void WriteChanges_WordBank()
        {
            // if any chemical compound records are flagged as modified, begin db transaction to write changes
            if (CheckModified_WordBank())
            {
                // create new file (overwriting old one)
                dbState.CreateNewDatabase();

                var outputBytes = new byte[25000];

                // write file contents into outputBytes
                long c = 0;

                // write data version as first line of output data text
                String dataVersionString = "" + WordSearchMakerVersion + "\n";
                var dataVersionBytes = dataVersionString.ToCharArray();
                for (int i = 0; i < dataVersionBytes.Length; i++)
                {
                    outputBytes[c + i] = (byte)dataVersionBytes[i];
                }
                c += dataVersionBytes.Length;

                // handle modified (non-deletion) records
                foreach (var word in wordBankCollection)
                {
                    if (!word.TextIsValid)
                    {
                        continue;
                    }

                    String wordTextString = word.Text + '\n';
                    var wordTextBytes = wordTextString.ToCharArray();
                    for (int i = 0; i < wordTextBytes.Length; i++)
                    {
                        outputBytes[c + i] = (byte)wordTextBytes[i];
                    }
                    c += wordTextBytes.Length;

                    word.Modified = false;
                }

                outputBytes[c] = (byte)'\n';

                int chunksToWrite = (int)(c / int.MaxValue) + 1;
                int remainder = (int)(c % int.MaxValue);

                for (int i = 0; i < chunksToWrite; i += int.MaxValue)
                {
                    var bytesToWrite = (i == chunksToWrite - 1) ? remainder : int.MaxValue;
                    var partialBytes = new byte[bytesToWrite];

                    // copy bytes from byte[intMax] container to the byte[longMax] container
                    for (int x = 0; x < bytesToWrite; x++)
                    {
                        partialBytes[(chunksToWrite - 1) * int.MaxValue + x] = outputBytes[x];
                    }

                    // note: length written is int, but length of FileStream is long
                    // this means that if the length of file is > int max the extra
                    // bytes will be dropped from writing. NEED TO FIX WHEN INTERNET IS POSSIBLE
                    object writeToken = (int)50;
                    var result = dbState.GetDatabase().BeginWrite(partialBytes, (chunksToWrite - 1) * int.MaxValue, bytesToWrite, null, writeToken);
                    dbState.GetDatabase().EndWrite(result);
                    dbState.GetDatabase().Close();
                }

                deletedWordIDs.Clear();
                WordBankCollectionModifiedViaDeletion = false;
            }
        }

        internal void AddNew_WordBank_TableItem()
        {
            if (wordBankCollection == null)
            {
                throw new Exception("Attempted to add blank row to null dose event records table");
            }
            Words.Add(CreateNew_Word());
        }

        internal Word CreateNew_Word()
        {
            var tableItem = new Word();
            tableItem.ID = ++latestwordBankID;
            return tableItem;
        }

        internal void Update_WordBank_Latest_Version()
        {

        }
    }
}
