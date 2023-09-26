using System;
using System.Linq;
using DrMuscle.iOS.Renderer;
using DrMuscle.iOS.Service;
using DrMuscle.Screens.Workouts;
using SlideOverKit.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExerciseVideoPage), typeof(ExerciseVideoPageRenderer))]

namespace DrMuscle.iOS.Renderer
{
    public class ExerciseVideoPageRenderer : MenuContainerPageiOSRenderer
    {
        public ExerciseVideoPageRenderer()
        {
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            new OrientationService().Landscape();

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            new OrientationService().Portrait();

        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            //if (App.Current.MainPage.Navigation.NavigationStack.Last() is ExerciseVideoPage)
            //    new OrientationService().Landscape();
            //else
            //new OrientationService().Portrait();
            new OrientationService().Portrait();
        }


    }
}
