using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using DrMuscle.Resx;
namespace DrMuscle.Screens.Workouts
{
    public partial class PinLockPage : DrMusclePage
    {
        string enteredText = "";
        Dictionary<string, Label> lDict = new Dictionary<string, Label>();
        public PinLockPage()
        {
            InitializeComponent();
            this.ToolbarItems.Clear();
            NavigationPage.SetBackButtonTitle(this, "");
           
        }

    }
}
