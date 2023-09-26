using System;
using DrMuscle.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(Styles_Droid))]
namespace DrMuscle.Droid
{
	public class Styles_Droid : IStyles
	{
		public int GetStyleId(EAlertStyles alertStyle)
		{
			switch (alertStyle)
			{
				default:
				case EAlertStyles.AlertDialogCustomGray:
					return Resource.Style.AlertDialogCustomGray;
				case EAlertStyles.AlertDialogCustomGreen:
					return Resource.Style.AlertDialogCustomGreen;
				case EAlertStyles.AlertDialogCustomRed:
					return Resource.Style.AlertDialogCustomRed;
                case EAlertStyles.AlertDialogFirstTimeExercise:
                    return Resource.Style.AlertDialogFirstTimeExercise;
			}
		}
	}
}
