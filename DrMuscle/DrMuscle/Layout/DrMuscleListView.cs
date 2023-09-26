using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DrMuscle.Layout
{
    public class DrMuscleListView : ListView
    {
        public DrMuscleListView() : base()
        {
            ItemTapped += DrMuscleListView_ItemTapped;
        }

        private void DrMuscleListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Analytics.TrackEvent($"Item [{e.Item.ToString()}] tapped");
            try
            {
                if (((ListView)sender).SelectedItem == null)
                    return;
                ((ListView)sender).SelectedItem = null;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
