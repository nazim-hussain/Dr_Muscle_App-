﻿using System;
using System.Linq;
using DrMuscle.Effects;
using DrMuscle.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("CubiSoft")]
[assembly: ExportEffect(typeof(DropShadowEffect), "DropShadowEffect")]
namespace DrMuscle.Droid
{
	public class DropShadowEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			try
			{
				var control = Control??Container as Android.Views.View;

				var effect = (ViewShadowEffect)Element.Effects.FirstOrDefault(e => e is ViewShadowEffect);

				if (effect != null)
				{
					float radius = effect.Radius;
					Android.Graphics.Color color = effect.Color.ToAndroid();
                    //control.SetShadowLayer(radius, distanceX, distanceY, color);

                    //int currentapiVersion = Android.OS.Build.VERSION.SdkInt;
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                    {
                        // Do something for lollipop and above versions
                        control.Elevation = radius;
                        control.TranslationZ = (effect.DistanceX + effect.DistanceY) / 2;
                    }
                    else
                    {
                        // do something for phones running an SDK before lollipop
                    }
                    
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Cannot set property on attached control. Error: {0}", ex.Message);
			}
		}

		protected override void OnDetached()
		{
		}
	}
}
