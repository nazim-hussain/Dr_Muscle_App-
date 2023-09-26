using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DrMuscle.Screens.History
{
    public partial class HistoryExerciseLogView : ContentView
    {
        public HistoryExerciseLogView()
        {
            InitializeComponent();
        }

        public void SetModel(HistoryExerciseModel model)
        {
            ExerciseLabel.Text = model.Exercise.Label;
            foreach (WorkoutLogSerieModel set in model.Sets)
            {
                Label setLabel = new Label() { TextColor = Color.White };
                setLabel.Text = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? string.Format("{0:0.0} kg", set.Weight.Kg) : string.Format("{0:0.0} lbs", set.Weight.Lb);
                setLabel.Text += string.Format(" {0} reps", set.Reps);
                SetsStackLayout.Children.Add(setLabel);
            }
            Best1RMLabel.Text = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? string.Format("{0:0.0} kg best 1RM", model.BestSerie1RM.Kg) : string.Format("{0:0.0} lbs best 1RM", model.BestSerie1RM.Lb);
            OverallLabel.Text = string.Format("{0:0.0} total ({1} sets | {2} reps | {3} per rep on average)",
                LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                    string.Format("{0:0.0} kg", model.TotalWeight.Kg) :
                    string.Format("{0:0.0} lbs", model.TotalWeight.Lb),
                model.Series,
                model.Reps,
                LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                    string.Format("{0:0.0} kg", model.AverageWeightByRep.Kg) :
                    string.Format("{0:0.0} lbs", model.AverageWeightByRep.Lb)
                );
        }
    }
}
