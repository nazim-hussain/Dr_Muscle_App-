using System;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;

[assembly: Dependency(typeof(DroidKeyboardHelper))]
namespace DrMuscle.Droid.Renderer
{
	public class DroidKeyboardHelper : IKeyboardHelper
	{

		public void HideKeyboard()
		{
			var context = Android.App.Application.Context;
			var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
			if (inputMethodManager != null )
			{
				try
				{

				var token = MainActivity._currentActivity.CurrentFocus?.WindowToken;
				inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

				//activity.Window.DecorView.ClearFocus();

                }
                catch (Exception ex)
                {

                }
            }
		}
	}
}
