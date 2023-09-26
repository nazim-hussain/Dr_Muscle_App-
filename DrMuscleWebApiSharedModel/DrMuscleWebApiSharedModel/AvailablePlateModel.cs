using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DrMuscleWebApiSharedModel
{
    public class AvailablePlateModel
    {
        private string _key;
        private int _value;
        public int Id { get; set; }
        public string Key
        {
            get;set;
        }
        public int Value
        {
            get;set;
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

