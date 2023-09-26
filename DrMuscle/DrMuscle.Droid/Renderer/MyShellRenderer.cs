using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Toolbar = Android.Support.V7.Widget.Toolbar;

//[assembly: ExportRenderer(typeof(Xamarin.Forms.Shell), typeof(MyShellRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class MyShellRenderer : ShellRenderer
    {
        public MyShellRenderer(Context context) : base(context)
        {
        }

        protected override IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker()
        {
            return new MyShellToolbarAppearanceTracker(this);
        }
        protected override IShellToolbarTracker CreateTrackerForToolbar(AndroidX.AppCompat.Widget.Toolbar toolbar)
        {
            return base.CreateTrackerForToolbar(toolbar);

        }

        //protected override IShellToolbarTracker CreateTrackerForToolbar(Toolbar toolbar)
        //{
        //    return new MyShellToolbarTracker(this, toolbar, ((IShellContext)this).CurrentDrawerLayout);
        //}
    }

    public class MyShellToolbarAppearanceTracker : ShellToolbarAppearanceTracker
    {
        public MyShellToolbarAppearanceTracker(IShellContext context) : base(context)
        {
        }

        public override void SetAppearance(AndroidX.AppCompat.Widget.Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
        {
            base.SetAppearance(toolbar, toolbarTracker, appearance);
        }
        //public override void SetAppearance(Android.Support.V7.Widget.Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
        //{
        //    base.SetAppearance(toolbar, toolbarTracker, appearance);
        //    //Change the following code to change the icon of the Header back button.

        //    //toolbar?.SetNavigationIcon(Resource.Drawable.Back);
        //}
    }

    //public class MyShellToolbarTracker : ShellToolbarTracker
    //{
    //    public MyShellToolbarTracker(IShellContext shellContext, Toolbar toolbar, DrawerLayout drawerLayout) : base(shellContext, toolbar, drawerLayout)
    //    {
    //    }

    //    protected override void UpdateTitleView(Context context, Toolbar toolbar, View titleView)
    //    {
    //        base.UpdateTitleView(context, toolbar, titleView);
    //        for (int index = 0; index < toolbar.ChildCount; index++)
    //        {
    //            if (toolbar.GetChildAt(index) is TextView)
    //            {
    //                var title = toolbar.GetChildAt(index) as TextView;
    //                //Change the following code to change the font size of the Header title.
    //                title.SetTextSize(ComplexUnitType.Sp, 20);
    //                toolbar.SetTitleMargin(MainActivity.displayMetrics.WidthPixels / 4 - Convert.ToInt32(title.TextSize) - title.Text.Length / 2, 0, 0, 0);
    //            }
    //        }
    //    }
    //}
}
