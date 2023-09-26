using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DrMuscle
{
    public class PlateModel : INotifyPropertyChanged
    {
        private string _key;
        private int _value;
        public int Id { get; set; }
        public string Key { get
            { 
                return _key; 
            }
            set 
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
                OnPropertyChanged(nameof(Label));
            } 
        }
        public int Value {
            get
            { 
                return _value; 
            }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(Label));
            }
        }
        public double Weight { get; set; }
        public bool IsSystemPlates { get; set; }
        public int CalculatedPlatesCount { get; set; }
        public int NotAvailablePlatesCount { get; set; }
        public bool isAvailable { get; set; }
        public string WeightType { get; set; }
        public string Label { get
            { return Id == -1 ? Key : $"{Key} {WeightType} x {Value}"; }
        }
        public override string ToString()
        {
            return $"{Key}_{Value}";
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DumbellModel : INotifyPropertyChanged
    {
        private string _key;
        private int _value;
        public int Id { get; set; }
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
                OnPropertyChanged(nameof(Label));
            }
        }
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(Label));
            }
        }
        public double Weight { get; set; }
        public bool IsSystemPlates { get; set; }
        public int CalculatedPlatesCount { get; set; }
        public int NotAvailablePlatesCount { get; set; }
        public bool isAvailable { get; set; }
        public string WeightType { get; set; }
        public string Label
        {
            get
            { return Id == -1 ? Key : $"{Key} {WeightType} x {Value}"; }
        }
        public override string ToString()
        {
            return $"{Key}_{Value}";
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PulleyModel : INotifyPropertyChanged
    {
        private string _key;
        private int _value;
        public int Id { get; set; }
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
                OnPropertyChanged(nameof(Label));
            }
        }
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(Label));
            }
        }
        public double Weight { get; set; }
        public bool IsSystemPlates { get; set; }
        public int CalculatedPlatesCount { get; set; }
        public int NotAvailablePlatesCount { get; set; }
        public bool isAvailable { get; set; }
        public string WeightType { get; set; }
        public string Label
        {
            get
            { return Id == -1 ? Key : $"{Key} {WeightType} x {Value}"; }
        }
        public override string ToString()
        {
            return $"{Key}_{Value}";
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
