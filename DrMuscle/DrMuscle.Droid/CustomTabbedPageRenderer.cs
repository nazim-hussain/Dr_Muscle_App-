using Android.Content;
using Android.Support.Design.Internal;
using Android.Views;
using Android.Widget;
using DrMuscle.Droid.CustomRenderers;
using Xamarin.Forms;
using DrMuscle.Message;
using Android.OS;
using System.ComponentModel;
using Android.Support.Design.Widget;
//using Android.Support.V4.Content;
using AndroidX.AppCompat.Content;
using System.Collections.Generic;
using DrMuscle.Helpers;
using System;
using AndroidX.Core.Content;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.Platform.Android;
using Google.Android.Material.BottomNavigation;
using static Android.Content.ClipData;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer))]
namespace DrMuscle.Droid.CustomRenderers
{
    public class CustomTabbedPageRenderer : TabbedPageRenderer
    {
        public CustomTabbedPageRenderer(Context context) : base(context) { }

        private Dictionary<TabData, BadgeView> _tabViews = new Dictionary<TabData, BadgeView>();
        private int _badgeId;
        private bool _bottomBarInit = false;
        int _prevSelectedIndex = 0;

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);
            try
            {

            if (ViewGroup != null && ViewGroup.ChildCount > 0)
            {

                Google.Android.Material.BottomNavigation.BottomNavigationMenuView bottomNavigationMenuView = FindChildOfType<Google.Android.Material.BottomNavigation.BottomNavigationMenuView>(ViewGroup);
                bottomNavigationMenuView.SetMinimumHeight(67);
                Google.Android.Material.BottomNavigation.BottomNavigationView bottomNavigationView = FindChildOfType<Google.Android.Material.BottomNavigation.BottomNavigationView>(ViewGroup);
                bottomNavigationView.SetMinimumHeight(67);
                bottomNavigationMenuView.SetPadding(2, 10, 2, 10);
                bottomNavigationView.SetBackgroundDrawable(ContextCompat.GetDrawable(Context, Resource.Drawable.navBottom));

                MainTabbedPage formsPage = (MainTabbedPage)Element;


                // bottomNavigationView.SetOnNavigationItemReselectedListener(new BottomSheetActions());


                try
                {
                    bottomNavigationView.ItemSelected += (sender, args) =>
                    {
                        switch (args.Item.ItemId)
                        {
                            //case 3: 
                                //Comment out when stop handling event
                                //((MainTabbedPage)e.NewElement).PresentModalPage();
                                //args.Handled = false;
                                //break;

                            default:
                                e.NewElement.CurrentPage = e.NewElement.Children[args.Item.ItemId];
                                args.Handled = true;
                                var icon = args.Item;

                                break;
                        }
                    };
                }
                catch (Exception ex)
                {

                }
                if (bottomNavigationMenuView != null)
                {
                    //var shiftMode = bottomNavigationMenuView.Class.GetDeclaredField("mShiftingMode");

                    //shiftMode.Accessible = true;
                    //shiftMode.SetBoolean(bottomNavigationMenuView, false);
                    //shiftMode.Accessible = false;
                    //shiftMode.Dispose();
                    bottomNavigationMenuView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;

                    for (var i = 0; i < bottomNavigationMenuView.ChildCount; i++)
                    {
                        var item = bottomNavigationMenuView.GetChildAt(i) as Google.Android.Material.BottomNavigation.BottomNavigationItemView;
                        if (!Xamarin.Essentials.DeviceInfo.Model.ToLower().Contains("m32"))
                        {
                            item.SetMinimumHeight(67);
                        }
                        if (item == null) continue;

                        item.SetPadding(2, 8, 2, 8);
                        item.SetShifting(false);
                        item.SetChecked(item.ItemData.IsChecked);

                        if (i > 0)
                        {
                                //for (int j = 0; j < item.ChildCount; j++)
                                //{
                                //    var child = item.GetChildAt(j);
                                //    System.Diagnostics.Debug.WriteLine(child.GetType());
                                //}
                            ImageView imageView = (ImageView)item.FindViewById(Resource.Id.navigation_bar_item_icon_view);
                            if (Xamarin.Essentials.DeviceInfo.Model.ToLower().Contains("m32"))
                            {
                                imageView.LayoutParameters.Width = 40;
                                imageView.LayoutParameters.Height = 40;
                            }
                                else
                                {
                                    imageView.LayoutParameters.Width = 80;
                                    imageView.LayoutParameters.Height = 80;
                                }

                            }
                        var tabData = formsPage.Tabs[0];
                        Google.Android.Material.BottomNavigation.BottomNavigationItemView tabItemView = (Google.Android.Material.BottomNavigation.BottomNavigationItemView)bottomNavigationMenuView.GetChildAt(i);

                        if (i == 2)
                        {
                            if (_badgeId == 0)
                                _badgeId = Android.Views.View.GenerateViewId();

                            BadgeView badgeTextView = new BadgeView(Context) { Id = _badgeId, BadgeCaption = string.Empty, BadgeColor = tabData.BadgeColor.ToAndroid() };

                            tabData.PropertyChanged += (sender, args) =>
                            {
                                TabData currentTabData = (TabData)sender;
                                BadgeView currentBadgeTextView = _tabViews[currentTabData];


                                if (currentBadgeTextView != null)
                                {
                                    currentBadgeTextView.BadgeColor = currentTabData.BadgeColor.ToAndroid();
                                    currentBadgeTextView.BadgeCaption = currentTabData.BadgeCaption > 0 ? currentTabData.BadgeCaption.ToString() : string.Empty;
                                }
                            };

                            if (!_tabViews.ContainsKey(tabData))
                            {
                                tabItemView.AddView(badgeTextView);
                                _tabViews.Add(tabData, badgeTextView);
                            }
                        }
                    }
                    UpdateFontSize();
                    MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
                    {
                        UpdateFontSize();
                    });



                    if (bottomNavigationMenuView.ChildCount > 0) bottomNavigationMenuView.UpdateMenuView();
                    //bottomNavigationView.SetShiftMode(false, false);



                }
            }

            }
            catch (Exception ex)
            {

            }
        }
        abstract class BottomSheetActions : Java.Lang.Object, Google.Android.Material.BottomNavigation.BottomNavigationView.IOnNavigationItemReselectedListener, Google.Android.Material.BottomNavigation.BottomNavigationView.IOnNavigationItemSelectedListener
        {
            
            public void OnClick(IDialogInterface dialog, int which)
            {
               // Console.WriteLine("Hello fox");
            }

            public void OnClick(Android.Views.View v)
            {

            }

            public void OnNavigationItemReselected(IMenuItem item)
            {
                if (item.Order == 4)
                    Xamarin.Forms.MessagingCenter.Send<MoreTappedMessage>(new MoreTappedMessage(), "MoreTappedMessage");
            }

            public bool OnNavigationItemSelected(IMenuItem item)
            {
                return true;

            }

            //void Google.Android.Material.BottomNavigation.BottomNavigationView.IOnNavigationItemReselectedListener.OnNavigationItemReselected(IMenuItem item)
            //{
            //    if (item.Order == 4)
            //        Xamarin.Forms.MessagingCenter.Send<MoreTappedMessage>(new MoreTappedMessage(), "MoreTappedMessage");
            //}

           
        }

        private void UpdateFontSize()
        {
            try
            {

                if (ViewGroup != null && ViewGroup.ChildCount > 0)
                {
                    Google.Android.Material.BottomNavigation.BottomNavigationMenuView bottomNavigationMenuView = FindChildOfType<Google.Android.Material.BottomNavigation.BottomNavigationMenuView>(ViewGroup);

                    if (bottomNavigationMenuView != null)
                    {

                        for (var i = 0; i < bottomNavigationMenuView.ChildCount; i++)
                        {
                            var item = bottomNavigationMenuView.GetChildAt(i) as Google.Android.Material.BottomNavigation.BottomNavigationItemView;
                            if (item == null) continue;


                            var itemTitle = item.GetChildAt(1);
                            var smallTextView = ((TextView)((BaselineLayout)itemTitle).GetChildAt(0));
                            var largeTextView = ((TextView)((BaselineLayout)itemTitle).GetChildAt(1));
                            
                            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") == null)
                            {
                                LocalDBManager.Instance.SetDBSetting("AppLanguage", "en");
                            }
                            if (LocalDBManager.Instance.GetDBSetting("AppLanguage").Value == "en")
                            {
                                smallTextView.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
                                largeTextView.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                {
                                    largeTextView.SetAutoSizeTextTypeUniformWithConfiguration(8, 15, 1, (int)Android.Util.ComplexUnitType.Sp);
                                }
                            }
                            else
                            {
                                smallTextView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);
                                largeTextView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                {
                                    largeTextView.SetAutoSizeTextTypeUniformWithConfiguration(8, 13, 1, (int)Android.Util.ComplexUnitType.Sp);
                                }

                            }

                        }

                        if (bottomNavigationMenuView.ChildCount > 0) bottomNavigationMenuView.UpdateMenuView();
                    }
                }

            }
            catch (System.Exception ex)
            {

            }
        }
        private T FindChildOfType<T>(ViewGroup viewGroup) where T : Android.Views.View
        {
            if (viewGroup == null || viewGroup.ChildCount == 0) return null;
            try
            {

                for (var i = 0; i < viewGroup.ChildCount; i++)
                {
                    var child = viewGroup.GetChildAt(i);

                    var typedChild = child as T;
                    if (typedChild != null) return typedChild;

                    if (!(child is ViewGroup)) continue;

                    var result = FindChildOfType<T>(child as ViewGroup);

                    if (result != null) return result;
                }

            }
            catch (System.Exception ex)
            {

            }
            return null;
        }


    }
}