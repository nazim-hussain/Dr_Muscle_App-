using System;
using DrMuscle.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(Styles_iOS))]
namespace DrMuscle.iOS
{
	public class Styles_iOS : IStyles
	{
		public int GetStyleId(EAlertStyles alertStyle)
		{
			return 0;
		}
	}
}
