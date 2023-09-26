using Acr.UserDialogs;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Resx;
using Xamarin.Forms;
using DrMuscle.Effects;
using DrMuscle.Constants;
using System.Threading;
using Rg.Plugins.Popup.Services;

namespace DrMuscle.Screens.Workouts
{
	public partial class ChooseWorkoutExerciseOrder : DrMusclePage
    {
        public ObservableListCollection<ExerciceModel> exerciseItems = new ObservableListCollection<ExerciceModel>();
        private View _toolTip;
        public ChooseWorkoutExerciseOrder()
        {
            InitializeComponent();

           

           // exerciseItems.OrderChanged += (sender, e) => {
           //         int jersey = 1;
                    //foreach (var item in _allContacts)
                    //{
                    //    item.Jersey = jersey++;
                    //}
                //};
            exerciseItems.CollectionChanged += (sender, e) =>
            {

            };
         }

    }
}
