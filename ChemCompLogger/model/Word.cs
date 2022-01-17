using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChemCompLogger.model
{
    public class Word : ICloneable, INotifyPropertyChanged
    {
        private long id;
        private long version;
        private string text;
        private bool editingError;

        public Word()
        {
            Text = string.Empty;
            TextIsUnset = true;
        }

        public long ID
        {
            get
            {
                return id;
            }
            set
            {
                if (value != id)
                {
                    id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public long Version
        {
            get
            {
                return version;
            }
            set
            {
                if (value != version)
                {
                    version = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Text
        {
            get
            {
                if (editingError)
                {
                    return text;
                }
                if (TextIsUnset)
                {
                    return text;
                }
                if (TextContainsInvalidChars)
                {
                    return "{{ Please use only letters A-Z }}";
                }
                if (TextIsInvalidLength)
                {
                    return "{{ Please enter a word 1-16 characters in length (spaces will be dropped) }}";
                }
                return text;
            }
            set
            {
                TextContainsInvalidChars = false;
                TextIsInvalidLength = false;
                if (value == null)
                {
                    value = string.Empty;
                    TextIsInvalidLength = true;
                }
                else
                {
                    text = ((string)value).ToUpper().Replace(" ", "");
                    TextIsUnset = false;
                    for (int c = 0; c < text.Length; c++)
                    {
                        if (text[c] < 'A' || text[c] > 'Z')
                        {
                            TextContainsInvalidChars = true;
                        }
                    }
                    if (text.Length == 0)
                    {
                        TextIsInvalidLength = true;
                    }
                    if (text.Length > 16)
                    {
                        TextIsInvalidLength = true;
                    }
                }
                NotifyPropertyChanged();
            }
        }

        internal void ResetText()
        {
            editingError = true;
            Text = text;
            editingError = false;
        }

        public bool TextContainsInvalidChars
        {
            get; private set;
        }

        public bool TextIsInvalidLength
        {
            get; private set;
        }

        public bool TextIsUnset
        {
            get; private set;
        }

        public bool TextIsValid
        {
            get
            {
                if (TextIsUnset) return false;
                if (TextContainsInvalidChars) return false;
                if (TextIsInvalidLength) return false;
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Modified
        {
            get; set;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Modified = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
