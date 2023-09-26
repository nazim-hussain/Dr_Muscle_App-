using System;
using Android.Content;
using DrMuscle.Droid.Services;
using DrMuscle.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(OpenImplementation))]
namespace DrMuscle.Droid.Services
{
    public class OpenImplementation : IOpenManager
    {
        public void openMail()
        {
            try
            {

            Intent intent = new Intent(Intent.ActionMain);
            intent.AddFlags(ActivityFlags.NewTask);
            intent.AddCategory(Intent.CategoryAppEmail);
            Android.App.Application.Context.StartActivity(intent);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
