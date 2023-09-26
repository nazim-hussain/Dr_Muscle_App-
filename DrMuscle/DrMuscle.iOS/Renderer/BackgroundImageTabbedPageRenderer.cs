using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using DrMuscle.Helpers;
using DrMuscle.iOS.Renderer;
using DrMuscle.Screens.Workouts;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(TabbedPage), typeof(BackgroundImageTabbedPageRenderer))]
namespace DrMuscle.iOS.Renderer
{

    public class BackgroundImageTabbedPageRenderer : TabbedRenderer
    {
        private Dictionary<TabData, View> _tabViews = new Dictionary<TabData, View>();

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            var image = UIImage.FromBundle("nav.png");

            image = image.Scale(new CGSize(TabBar.Frame.Width, TabBar.Frame.Height));
            TabBar.BackgroundImage = image;
            TabBar.UnselectedItemTintColor = UIColor.FromRGBA(255, 255, 255, 150);
            TabBar.ShadowImage = new UIImage();
            TabBar.Translucent = true;
            
            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                var appearance = new UITabBarAppearance();
                appearance.ConfigureWithOpaqueBackground();
                appearance.BackgroundImage = image;
                TabBar.StandardAppearance = appearance;
                TabBar.ScrollEdgeAppearance = appearance;
            }
            foreach (var item in TabBar.Items)
            {
                item.TitlePositionAdjustment = new UIOffset(10, 10);
            }
            //UITextAttributes normalTextAttributes = new UITextAttributes();
            //normalTextAttributes.Font = UIFont.SystemFontOfSize(20); // unselected
            //UITabBarItem.Appearance.SetTitleTextAttributes(normalTextAttributes, UIControlState.Normal);
            //UITabBarItem.Appearance.SetTitleTextAttributes(normalTextAttributes, UIControlState.Selected);
            //UITabBarItem.Appearance.SetTitleTextAttributes(normalTextAttributes, UIControlState.Highlighted);
        }


        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            var newHeight = TabBar.Frame.Height + 7;
            CoreGraphics.CGRect tabFrame = TabBar.Frame;
            tabFrame.Height = newHeight;
            tabFrame.Y = View.Frame.Size.Height - newHeight;
            TabBar.Frame = tabFrame;

            foreach (UIViewController vc in ViewControllers)
            {
                //Adjust the title's position   
                vc.TabBarItem.TitlePositionAdjustment = new UIOffset(0.0f, 1.0f);
                //Adjust the icon's position
                // vc.TabBarItem.ImageInsets = new UIEdgeInsets(0, 0, 0, 5);
                var txtFont = new UITextAttributes()
                {
                    Font = UIFont.BoldSystemFontOfSize(20)
                };
                foreach (var controller in ViewControllers)
                {
                    controller.TabBarItem.SetTitleTextAttributes(txtFont, UIControlState.Normal);
                }
            }
        }


        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            //if (TabBar.Items != null)
            //{
            //    var items = TabBar.Items;
            //    foreach (var t in items)
            //    {
            //        var txtSelectedColor = new UITextAttributes()
            //        {
            //            TextColor = UIColor.Orange
            //        };

            //        var txtFont = new UITextAttributes()
            //        {
            //            Font = UIFont.SystemFontOfSize(18)
            //        };

            //        t.SetTitleTextAttributes(txtSelectedColor, UIControlState.Normal);
            //        t.SetTitleTextAttributes(txtFont, UIControlState.Normal);

            //    }

            //}
        }

        //public override UIViewController SelectedViewController
        //{
        //    get
        //    {
        //        UITextAttributes selectedTextAttributes = new UITextAttributes();
        //        selectedTextAttributes.Font = UIFont.SystemFontOfSize(20); // SELECTED
        //        if (base.SelectedViewController != null)
        //        {
        //            base.SelectedViewController.TabBarItem.SetTitleTextAttributes(selectedTextAttributes, UIControlState.Normal);
        //        }
        //        return base.SelectedViewController;
        //    }
        //    set
        //    {
        //        base.SelectedViewController = value;

        //        foreach (UIViewController viewController in base.ViewControllers)
        //        {
        //            UITextAttributes normalTextAttributes = new UITextAttributes();
        //            normalTextAttributes.Font = UIFont.SystemFontOfSize(20); // unselected
        //            normalTextAttributes.TextColor = UIKit.UIColor.Blue;
        //            viewController.TabBarItem.SetTitleTextAttributes(normalTextAttributes, UIControlState.Normal);
        //        }
        //    }
        //}
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.ShouldSelectViewController += (tabBarController, viewController) =>
            {
                bool canMove;

                

                //if (viewController is NavigationRenderer)
                //{
                //    var controller = viewController as NavigationRenderer;
                //    System.Diagnostics.Debug.WriteLine(controller.Element);
                //    if (controller.Element is NavigationPage)
                //    {
                //        var navi = controller.Element as NavigationPage;
                //        System.Diagnostics.Debug.WriteLine(navi.Navigation.NavigationStack.FirstOrDefault());
                //        System.Diagnostics.Debug.WriteLine(navi.Navigation.NavigationStack.LastOrDefault());
                //    }

                //}
                    canMove = (tabBarController.SelectedViewController != viewController);
                
                return canMove;
            };
            AutomaticallyAdjustsScrollViewInsets = false;
        }



        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            try
            {


                MainTabbedPage formsPage = (MainTabbedPage)Element;
                var tabData = formsPage.Tabs[0];

                if (formsPage.Children.Count > 0)
                {
                    tabData.PropertyChanged += (sender, args) =>
                    {
                        TabData currentTabData = (TabData)sender;
                        UITabBarItem tabBarItemChat = TabBar.Items[2];

                        if (currentTabData.BadgeCaption > 0)
                        {
                            AddRedDotAtTabBarItemIndex(2);
                        }
                        else
                        {
                            RemoveDot();
                        }
                    };
                }
            }
            catch (Exception ex)
            {

            }
        }

        void RemoveDot()
        {
            try
            {


                foreach (var item in TabBar.Subviews)
                {
                    if (item is UIView)
                    {
                        if (item.Tag == 1234)
                        {
                            item.RemoveFromSuperview();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        void AddRedDotAtTabBarItemIndex(int index)
        {
            try
            {


                foreach (var item in TabBar.Subviews)
                {
                    if (item is UIView)
                    {
                        if (item.Tag == 1234)
                        {
                            item.RemoveFromSuperview();
                            break;
                        }
                    }
                }
                var reddotRadius = 5;
                var topmargin = 5;
                var redDotDiameter = reddotRadius * 2;

                var TabBarItemCount = TabBar.Items.Length;
                var screenSize = UIScreen.MainScreen.Bounds;
                var HalfItemWidth = (screenSize.Width) / (TabBarItemCount * 2);
                var xOffset = HalfItemWidth * (index * 2 + 1);
                var imageHalfWidth = TabBar.Items[index].SelectedImage.Size.Width / 2;
                var redDot = new UIView(new CGRect(xOffset + imageHalfWidth - 7, topmargin, redDotDiameter, redDotDiameter));
                redDot.Tag = 1234;
                redDot.BackgroundColor = UIColor.Red;
                redDot.Layer.CornerRadius = reddotRadius;
                TabBar.AddSubview(redDot);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
