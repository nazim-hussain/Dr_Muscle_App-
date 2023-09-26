using System;
using System.ComponentModel;
using Android.Content;
using Android.Views.InputMethods;
using DrMuscle.Droid.Renderer;
using DrMuscle.Layout;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DrMuscleListViewCache), typeof(ListViewCacheRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class ListViewCacheRenderer : ListViewRenderer
    {
        private int countCaller = 0;
        InputMethodManager inputMethodManager;

        public ListViewCacheRenderer(Context context) : base(context)
        {
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            try
            {
                
                GetInputMethodManager();
                var method = inputMethodManager.Class.GetMethod("getInputMethodWindowVisibleHeight");
                int height = (int)method.Invoke(inputMethodManager);
                if (height > 100) {
                } else
                    base.OnLayout(changed, l, t, r, b);
            } catch
            {
                base.OnLayout(changed, l, t, r, b);
            }
        }

        private void GetInputMethodManager()
        {
            if (inputMethodManager == null || inputMethodManager.Handle == IntPtr.Zero)
            {
                inputMethodManager = (InputMethodManager)MainActivity._currentActivity.GetSystemService(Context.InputMethodService);
            }
        }
        //protected override void OnLayout(bool changed, int l, int t, int r, int b)
        //{
        //    if (changed)
        //    {
        //        base.OnLayout(changed, l, t, r, b);
        //    }
        //    else
        //    {
        //        if (countCaller <= 4)
        //        {

        //            base.OnLayout(changed, l, t, r, b);
        //            countCaller++;
        //        }
        //    }
        //}

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                var view = (DrMuscleListViewCache)Element;
                Control.FastScrollEnabled = true;
                Control.SmoothScrollbarEnabled = true;
                view.EventScrollToTop += View_EventScrollToTop;
            }
        }

        private void View_EventScrollToTop(object sender, EventArgs e)
        {
            try
            {
                if (Control != null)
                    Control.SmoothScrollToPosition(0);
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            
            countCaller = 0;
            
            if (Control != null && e.PropertyName.Equals("IsScrolled"))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                   
                    try
                    {

                        DrMuscleListViewCache list = (DrMuscleListViewCache)Element;
                        //Control.SetSelection(list.ItemPosition);//
                        //Control.TranscriptMode = Android.Widget.TranscriptMode.Disabled;
                        Control.SmoothScrollToPositionFromTop(list.ItemPosition+1,0);
                        
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            if (Control != null)
                Control.InvalidateViews();
        }
        public void scrollToTop()
        {
            
            
        }
    }
}
