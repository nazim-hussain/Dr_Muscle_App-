using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;

namespace DrMuscle
{
    public delegate string GetIndexEvent();
	public class WorkoutLogSerieModelEx : WorkoutLogSerieModel, INotifyPropertyChanged
	{
        [JsonIgnore]
        public GetIndexEvent OnGetIndex;
		public string SetLabel
		{
			get
			{
				return string.Format("{0}. {1:0.0} {2} x {3} {4}",
									 Index,
									 LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? Weight.Kg : Weight.Lb,
									 LocalDBManager.Instance.GetDBSetting("massunit").Value,
									 Reps, CurrentLog.Instance.ExerciseLog.Exercice.IsTimeBased ? Reps <= 1 ? "sec" : "secs" : Reps <= 1 ? "rep" : "reps").Replace(",",".");
			}
		}
        [JsonIgnore]
        public string Index
		{
			get
			{
				if (OnGetIndex != null)
					return OnGetIndex();
				return "";
			}
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
