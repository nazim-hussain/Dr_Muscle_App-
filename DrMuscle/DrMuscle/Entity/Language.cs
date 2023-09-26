using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace DrMuscle.Entity
{
    public class Language : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected { 
        get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string LanguageName { get; set; }
        public string LanguageCode { get; set; }
        public ImageSource FlagImage { get; set; }


        public Language(string flag, string languageName, string code, bool isSelected)
        {
            FlagImage = flag;
            LanguageName = languageName;
            LanguageCode = code;
            IsSelected = isSelected;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetObservableProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
