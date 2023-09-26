using System;
using Xamarin.Forms;
using DrMuscle.Layout;
using DrMuscle.Droid.Renderers;
using SlideOverKit.Droid;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using System.ComponentModel;
using Android.Views;
using DrMuscle.Screens.User;
using DrMuscle.Screens.User.OnBoarding;
using Android.App;
using Android.Content;

[assembly: ExportRenderer(typeof(DrMusclePage), typeof(DrMusclePageRenderer))]
namespace DrMuscle.Droid.Renderers
{
    public class DrMusclePageRenderer : MenuContainerPageDroidRenderer
    {
        Android.Widget.ImageView imageView;
        Android.Widget.RelativeLayout frameLayout;
        public DrMusclePageRenderer(Context context) : base(context)
        {
            new SlideOverKitDroidHandler().Init(this, context);
       
            //MessagingCenter.Subscribe<SettingsPage>(this, "BackgroundImageUpdated", BackgroundImageChanged);
            //MessagingCenter.Subscribe<MainAIPage>(this, "BackgroundImageUpdated", BackgroundWelcomePageImageChanged);
            //MessagingCenter.Subscribe<OnboardingPage1Welcome>(this, "BackgroundImageUpdated", BackgroundOnboardingWelcomePageImageChanged);
        }


        //protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        //{
        //    base.OnElementChanged(e);
        //    if (Element == null)
        //        return;
        //    try
        //    {

        //        imageView = new Android.Widget.ImageView(Context);
        //        frameLayout = new Android.Widget.RelativeLayout(this.Context);
        //        Android.Widget.RelativeLayout.LayoutParams layout1 = new Android.Widget.RelativeLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent);
        //        imageView.LayoutParameters = layout1;
        //        BackgroundImageChanged(null);
        //        frameLayout.AddView(imageView);
        //        Android.Widget.RelativeLayout.LayoutParams layout = new Android.Widget.RelativeLayout.LayoutParams(Android.Widget.RelativeLayout.LayoutParams.MatchParent, Android.Widget.RelativeLayout.LayoutParams.MatchParent);

        //        frameLayout.LayoutParameters = layout;
        //        try
        //        {
        //            AddView(frameLayout);
        //            System.Diagnostics.Debug.WriteLine($"Z index{frameLayout.GetZ()}");
        //            //frameLayout.SetZ(1);

        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //        //var toolbar = (MainActivity._currentActivity as MainActivity)?.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
        //        //if (toolbar != null)
        //        //{
        //        //for (var i = 0; i < toolbar.ChildCount; i++)
        //        //{
        //        //    var Title = toolbar.GetChildAt(i);

        //        //    if (Title is TextView)
        //        //    {
        //        //        var title = toolbar.GetChildAt(i) as TextView;

        //        //        title.Gravity = GravityFlags.Center;
        //        //        title.TextAlignment = Android.Views.TextAlignment.Center;

        //        //        LinearLayout.LayoutParams txvPars = (LinearLayout.LayoutParams)title.LayoutParameters;
        //        //        txvPars.Gravity = GravityFlags.CenterHorizontal;
        //        //        txvPars.Width = MainActivity.displayMetrics.WidthPixels;
        //        //        title.LayoutParameters = txvPars;
        //        //        title.Gravity = GravityFlags.Center;
        //        //    }
        //        //}
        //        //}
                
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //protected override void OnAttachedToWindow()
        //{
        //    base.OnAttachedToWindow();

        //}

        //protected override void OnDetachedFromWindow()
        //{
        //    base.OnDetachedFromWindow();
        //    MessagingCenter.Unsubscribe<RightSideMasterPage>(this, "BackgroundImageUpdated");

        //}

        //protected override void OnLayout(bool changed, int l, int t, int r, int b)
        //{
        //    try
        //    {

        //    base.OnLayout(changed, l, t, r, b);
        //    if (!changed)
        //        return;
        //        var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
        //    var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);
        //    frameLayout.Measure(msw, msh);
        //    frameLayout.Layout(0, 0, r - l, b - t);

        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        void BackgroundImageChanged(SettingsPage obj)
        {
            ChangeBackground();
        }

        void BackgroundWelcomePageImageChanged(MainAIPage obj)
        {
            ChangeBackground();
        }
        void BackgroundOnboardingWelcomePageImageChanged(MainAIPage obj)
        {
            ChangeBackground();
        }

        private void ChangeBackground()
        {
            if (LocalDBManager.Instance.GetDBSetting("BackgroundImage") == null)
            {
                imageView.SetImageResource(Resource.Drawable.Backgroundblack);
                return;
            }

            switch (LocalDBManager.Instance.GetDBSetting("BackgroundImage").Value)
            {
                //case "Background2.png":
                //    imageView.SetScaleType(ImageView.ScaleType.Center);
                //    imageView.SetImageResource(Resource.Drawable.Background2);
                //    break;
                //case "BackgroundFemale.png":
                //    imageView.SetScaleType(ImageView.ScaleType.FitCenter);
                //    imageView.SetImageResource(Resource.Drawable.BackgroundFemale);
                //    break;
                //case "DrMuscleLogo.png":
                //    imageView.SetScaleType(ImageView.ScaleType.FitCenter);
                //    imageView.SetImageResource(Resource.Drawable.DrMuscleLogo);
                //    break;
                default:
                    imageView.SetImageResource(Resource.Drawable.Backgroundblack);
                    break;
            }
        }
    }
}
