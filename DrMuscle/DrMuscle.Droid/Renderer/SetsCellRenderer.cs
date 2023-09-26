using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using DrMuscle.Cells;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//[assembly: ExportRenderer(typeof(SetsCell), typeof(SetsCellRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class SetsCellRenderer : ViewCellRenderer
    {
        protected override Android.Views.View GetCellCore(Cell item, Android.Views.View convertView, ViewGroup parent, Context context)
        {
            return base.GetCellCore(item, convertView, parent, context);

        }

        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCellPropertyChanged(sender, e);
        }
    }
}
