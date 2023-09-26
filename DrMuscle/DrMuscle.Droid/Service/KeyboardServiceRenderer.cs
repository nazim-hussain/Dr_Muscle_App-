using System;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(DrMuscle.Droid.Service.KeyboardService))]

namespace DrMuscle.Droid.Service
{
    public class KeyboardService : Dependencies.IKeyboardService
    {
        public event EventHandler KeyboardIsShown;
        public event EventHandler KeyboardIsHidden;

        private InputMethodManager inputMethodManager;

        private bool wasShown = false;

        public KeyboardService()
        {
            GetInputMethodManager();
            SubscribeEvents();
        }

        public void OnGlobalLayout(object sender, EventArgs args)
        {
            GetInputMethodManager();
            
            
        }

        private bool IsCurrentlyShown()
        {
            return inputMethodManager.IsAcceptingText;
        }

        private void GetInputMethodManager()
        {
            if (inputMethodManager == null || inputMethodManager.Handle == IntPtr.Zero)
            {
                inputMethodManager = (InputMethodManager)MainActivity._currentActivity.GetSystemService(Context.InputMethodService);
            }
        }

        private void SubscribeEvents()
        {
           // ((Activity)Xamarin.Forms.Forms.Context).Window.DecorView.ViewTreeObserver.GlobalLayout += this.OnGlobalLayout;
        }

        public bool isCurrentlyShowing()
        {
           // inputMethodManager = (InputMethodManager)MainActivity._currentActivity.GetSystemService(Context.InputMethodService);
            //inputMethodWindowVisibleHeight
            //
            try
            {
                if (inputMethodManager == null || inputMethodManager.Handle == IntPtr.Zero)
                {
                    inputMethodManager = (InputMethodManager)MainActivity._currentActivity.GetSystemService(Context.InputMethodService);
                }
                var method = inputMethodManager.Class.GetMethod("getInputMethodWindowVisibleHeight");
                int height = (int)method.Invoke(inputMethodManager);
            if (height > 100)
            { // Value should be less than keyboard's height 
                return true;
            }
            else
            {
                return false;
                
            }

            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
    }
}