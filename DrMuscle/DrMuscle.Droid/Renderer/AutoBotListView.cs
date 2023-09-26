using System;
using System.ComponentModel;
using Android.Content;
using Xamarin.Forms;
using DrMuscle.Controls;
using Xamarin.Forms.Platform.Android;
using DrMuscle.Droid.Renderer;
using Android.Animation;

[assembly: ExportRenderer(typeof(AutoBotListView), typeof(AutoBotListViewRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class AutoBotListViewRenderer : ListViewRenderer
    {
        public AutoBotListViewRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (Control != null)
            {
                try
                {
                    var listview = this.Control as Android.Widget.ListView;
                    var animator  = listview.Animate();
                    animator.Start();


                }
                catch (Exception ex)
                {

                }

            }
        }
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                try
                {
                    
                    var listview = this.Control as Android.Widget.ListView;
                    listview.TranscriptMode = Android.Widget.TranscriptMode.AlwaysScroll;
                    
                    AutoBotListView list = (AutoBotListView)Element;
                    if (list.IsOnBoarding)
                    { 
                        listview.VerticalFadingEdgeEnabled = true;
                        LayoutTransition lt = new LayoutTransition();
                        lt.SetDuration(1000);
                        lt.EnableTransitionType(LayoutTransitionType.ChangeAppearing);
                        listview.LayoutTransition = lt;
                    }
                }
                catch (Exception ex)
                {

                }

            }
        }
    }
}
