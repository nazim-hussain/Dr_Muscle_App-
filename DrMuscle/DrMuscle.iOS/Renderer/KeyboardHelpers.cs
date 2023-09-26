using System;
using DrMuscle.iOS.Renderer;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardHelpers))]
namespace DrMuscle.iOS.Renderer
{
    public class KeyboardHelpers : IKeyboardHelper
    {
        
        public void HideKeyboard()
        {
            UIApplication.SharedApplication.KeyWindow.EndEditing(true);
        }
    }
}
