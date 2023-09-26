using DrMuscle.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DrMuscle.Helpers
{
    public static class PagesFactory
    {
        static readonly Dictionary<Type, Page> pages = new Dictionary<Type, Page>();
        static readonly MainTabbedPage _rootPage = GetTabbedPage<MainTabbedPage>();
        //static readonly MainPage _rootPage = GetPage<MainPage>();
        static NavigationPage navigation;


        public static NavigationPage GetNavigation(bool isMainRoot = false)
        {
            if (navigation == null)
            {
                navigation = new NoAnimationNavigationPage(_rootPage);
                navigation.BackgroundImageSource = "nav.png";
                navigation.BarTextColor = Color.White;
                navigation.PoppedToRoot += (object sender, NavigationEventArgs e) =>
                {
                    if (((NoAnimationNavigationPage)sender).CurrentPage is DrMusclePage)
                        ((DrMusclePage)((NoAnimationNavigationPage)sender).CurrentPage).OnShow();
                    if (((NoAnimationNavigationPage)sender).CurrentPage is MainTabbedPage)
                        ((MainTabbedPage)((NoAnimationNavigationPage)sender).CurrentPage).OnBeforeShow();
                };
                navigation.Pushed += (object sender, NavigationEventArgs e) =>
                {
                    //((DrMusclePage)((NoAnimationNavigationPage)sender).CurrentPage).OnShow();
                    if (((NoAnimationNavigationPage)sender).CurrentPage is DrMusclePage)
                        ((DrMusclePage)((NoAnimationNavigationPage)sender).CurrentPage).OnShow();
                };
                navigation.Popped += (object sender, NavigationEventArgs e) =>
                {
                    // ((DrMusclePage)((NoAnimationNavigationPage)sender).CurrentPage).OnShow();
                    if (((NoAnimationNavigationPage)sender).CurrentPage is DrMusclePage)
                        ((DrMusclePage)((NoAnimationNavigationPage)sender).CurrentPage).OnShow();
                };

            }
            else
            {
                try
                {
                    if (isMainRoot)
                        return navigation;
                    return ((NavigationPage)((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage);
                    //return ((DrMusclePage)((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage.Navigation;
                }
                catch (Exception ex)
                {

                }
            }
            return navigation;
        }
        public static T GetTabbedPage<T>(bool cachePages = true) where T : MainTabbedPage
        {
            Type pageType = typeof(T);

            if (cachePages)
            {
                if (!pages.ContainsKey(pageType))
                {
                    MainTabbedPage page = (MainTabbedPage)Activator.CreateInstance(pageType);
                    pages.Add(pageType, page);
                }

                return pages[pageType] as T;
            }
            else
            {
                return Activator.CreateInstance(pageType) as T;
            }
        }

        public static T GetPage<T>(bool cachePages = true) where T : DrMusclePage
        {
            Type pageType = typeof(T);

            if (cachePages)
            {
                if (!pages.ContainsKey(pageType))
                {
                    DrMusclePage page = (DrMusclePage)Activator.CreateInstance(pageType);
                    pages.Add(pageType, page);
                }

                return pages[pageType] as T;
            }
            else
            {
                return Activator.CreateInstance(pageType) as T;
            }
        }


        public static async Task PushMainTabbed<T>() where T : MainTabbedPage
        {
            if (!navigation.Navigation.NavigationStack.Any(p => p.GetType() == typeof(T)))
            {
                MainTabbedPage p = GetTabbedPage<T>();
                //p.OnBeforeShow();
                await GetNavigation(true).PushAsync(p);
            }
            else
            {
                await PopToPage<T>(true);
            }
        }

        public static async Task PushAsync<T>(bool isMainRoot = false) where T : DrMusclePage
        {
            if (!GetNavigation(isMainRoot).Navigation.NavigationStack.Any(p => p.GetType() == typeof(T)))
            {
                DrMusclePage p = GetPage<T>();
                p.OnBeforeShow();
                GetNavigation(isMainRoot).BackgroundImageSource = "nav.png";
                await GetNavigation(isMainRoot).PushAsync(p);
            }
            else
            {
                await PopToPage<T>(isMainRoot);
            }
        }

        public static async Task PushAsyncWithoutBefore<T>(bool isMainRoot = false) where T : DrMusclePage
        {
            if (!GetNavigation(isMainRoot).Navigation.NavigationStack.Any(p => p.GetType() == typeof(T)))
            {
                DrMusclePage p = GetPage<T>();
                p.OnBeforeShow();
                GetNavigation(isMainRoot).BackgroundImageSource = "nav.png";
                await GetNavigation(isMainRoot).PushAsync(p);
            }
            else
            {
                await PopToPageWithoutBefore<T>(isMainRoot);
            }
        }

        public static async Task PushMoveAsync<T>(bool isMainRoot = false) where T : DrMusclePage
        {
            if (!GetNavigation(isMainRoot).Navigation.NavigationStack.Any(p => p.GetType() == typeof(T)))
            {
                DrMusclePage p = GetPage<T>();
                p.OnBeforeShow();
                await GetNavigation(isMainRoot).PushAsync(p, false);
            }
            else
            {
                await PopToPage<T>(isMainRoot);
            }
        }
        public static async Task PopToBoardingPage<T>(bool isMainRoot = false)
        {

            // Remove page before Edit Page
            var nav = GetNavigation(isMainRoot).Navigation;
nav.RemovePage(GetNavigation(isMainRoot).Navigation.NavigationStack[nav.NavigationStack.Count - 3]);
            // This PopAsync will now go to List Page
            await PagesFactory.PopToPage<Screens.User.OnBoarding.MainOnboardingPage>();
            //}
            //if (GetNavigation(isMainRoot).CurrentPage is DrMusclePage)
            //{
            //    DrMusclePage p = GetNavigation().CurrentPage as DrMusclePage;
            //    p.OnBeforeShow();
            //}
        }

        public static async Task PopToPage<T>(bool isMainRoot = false)
        {
            
                while (GetNavigation(isMainRoot).CurrentPage.GetType() != typeof(T))
                {
                    await GetNavigation(isMainRoot).PopAsync(false);
                }
                if (GetNavigation(isMainRoot).CurrentPage is DrMusclePage)
                {
                    DrMusclePage p = GetNavigation().CurrentPage as DrMusclePage;
                    p.OnBeforeShow();
                }
            
        }

        public static async Task PopToPageWithoutBefore<T>(bool isMainRoot = false)
        {

            while (GetNavigation(isMainRoot).CurrentPage.GetType() != typeof(T))
            {
                await GetNavigation(isMainRoot).PopAsync(true);
            }
            

        }

        public static async Task PopAsync(bool isMainRoot = false)
        {
            await GetNavigation(isMainRoot).PopAsync();
        }

        public static async Task PopToRootAsync(bool isMainRoot = false)
        {
            //_rootPage.OnBeforeShow();
            await GetNavigation(isMainRoot).PopToRootAsync();
        }
        public static async Task PopToRootMoveAsync(bool isMainRoot = false)
        {
            //_rootPage.OnBeforeShow();
            await GetNavigation(isMainRoot).PopToRootAsync(false);
        }

        public static async Task PopThenPushAsync<T>(bool isMainRoot = false) where T : DrMusclePage
        {
            await GetNavigation(isMainRoot).PopAsync();
            DrMusclePage p = GetPage<T>();
            p.OnBeforeShow();
            await GetNavigation(isMainRoot).PushAsync(p);
        }

        public static async Task PopToRootThenPushAsync<T>(bool isMainRoot = false) where T : DrMusclePage
        {
            DrMusclePage p = GetPage<T>();
            p.OnBeforeShow();
            await ((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).Navigation.NavigationStack.First()).CurrentPage.Navigation.PushAsync(p);
            //var nav = GetNavigation(isMainRoot).Navigation;
            //try
            //{
            //    ((DrMusclePage)GetNavigation(true).Navigation.NavigationStack.First()).OnBeforeShow();
            //}
            //catch (Exception ex)
            //{

            //}

            //GetNavigation(isMainRoot).Navigation.InsertPageBefore(p, nav.NavigationStack[1]);
            //var count = nav.NavigationStack.Count;
            //for (int i = 2; i < count; i++)
            //{
            //    nav.RemovePage(nav.NavigationStack[2]);
            //}
            //await PagesFactory.PopToRootAsync(true);
            //await PagesFactory.PushMoveAsync<ChooseYourWorkoutExercisePage>();
        }
    }
}
