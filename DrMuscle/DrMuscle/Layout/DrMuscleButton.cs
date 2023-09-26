using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DrMuscle.Layout
{
    public class DrMuscleButton : Button
    {
        public DrMuscleButton() : base ()
        {
            Clicked += DrMuscleButton_Clicked;
        }

        private void DrMuscleButton_Clicked(object sender, EventArgs e)
        {
            Analytics.TrackEvent($"Button [{this.Text}] clicked");
        }
    }
}
