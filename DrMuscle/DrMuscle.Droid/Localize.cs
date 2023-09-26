using System;
using System.Globalization;
using System.Threading;
using DrMuscle.Localize;
using Xamarin.Forms;

[assembly: Dependency(typeof(DrMuscle.Droid.Localize))]
namespace DrMuscle.Droid
{
	public class Localize : ILocalize
	{
        string _appLanguage;
		public Localize()
		{
            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") == null)
            {
                LocalDBManager.Instance.SetDBSetting("AppLanguage", "en");
            }
            string cultureName = LocalDBManager.Instance.GetDBSetting("AppLanguage").Value;
			var locale = new Java.Util.Locale(cultureName);
			Java.Util.Locale.Default = locale;

			var config = new Android.Content.Res.Configuration { Locale = locale };

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value);
            var context = global::Android.App.Application.Context;
            context.Resources.UpdateConfiguration(config, context.Resources.DisplayMetrics);
            SetLocale(new CultureInfo(cultureName));
        }
        public void SetLocale(CultureInfo ci)
		{
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;

            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") == null)
            {
                LocalDBManager.Instance.SetDBSetting("AppLanguage", "en");
            }

            string cultureName = LocalDBManager.Instance.GetDBSetting("AppLanguage").Value;
            var locale = new Java.Util.Locale(cultureName);
            Java.Util.Locale.Default = locale;

            var config = new global::Android.Content.Res.Configuration { Locale = locale };
            var context = global::Android.App.Application.Context;
            context.Resources.UpdateConfiguration(config, context.Resources.DisplayMetrics);
        }

		public CultureInfo GetCurrentCultureInfo()
		{
            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") == null)
            {
                LocalDBManager.Instance.SetDBSetting("AppLanguage", "en");
            }
            var netLanguage = LocalDBManager.Instance.GetDBSetting("AppLanguage").Value;
			var androidLocale = Java.Util.Locale.Default;
			netLanguage = AndroidToDotnetLanguage(androidLocale.ToString().Replace("_", "-"));

			// this gets called a lot - try/catch can be expensive so consider caching or something
			System.Globalization.CultureInfo ci = null;
			try
			{
				ci = new System.Globalization.CultureInfo(netLanguage);
			}
			catch (CultureNotFoundException e1)
			{
				// iOS locale not valid .NET culture (eg. "en-ES" : English in Spain)
				// fallback to first characters, in this case "en"
				try
				{
					var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
					Console.WriteLine(netLanguage + " failed, trying " + fallback + " (" + e1.Message + ")");
					ci = new System.Globalization.CultureInfo(fallback);
				}
				catch (CultureNotFoundException e2)
				{
					// iOS language not valid .NET culture, falling back to English
					Console.WriteLine(netLanguage + " couldn't be set, using 'en' (" + e2.Message + ")");
					ci = new System.Globalization.CultureInfo("en");
				}
			}

			return ci;
		}

		string AndroidToDotnetLanguage(string androidLanguage)
		{
			Console.WriteLine("Android Language:" + androidLanguage);
			var netLanguage = androidLanguage;

			//certain languages need to be converted to CultureInfo equivalent
			switch (androidLanguage)
			{
				case "ms-BN":   // "Malaysian (Brunei)" not supported .NET culture
				case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
				case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
					netLanguage = "ms"; // closest supported
					break;
				case "in-ID":  // "Indonesian (Indonesia)" has different code in  .NET 
					netLanguage = "id-ID"; // correct code for .NET
					break;
				case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
					netLanguage = "de-CH"; // closest supported
					break;
					// add more application-specific cases here (if required)
					// ONLY use cultures that have been tested and known to work
			}

			Console.WriteLine(".NET Language/Locale:" + netLanguage);
			return netLanguage;
		}
		string ToDotnetFallbackLanguage(PlatformCulture platCulture)
		{
			Console.WriteLine(".NET Fallback Language:" + platCulture.LanguageCode);
			var netLanguage = platCulture.LanguageCode; // use the first part of the identifier (two chars, usually);

			switch (platCulture.LanguageCode)
			{
				case "gsw":
					netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
					break;

			}

			Console.WriteLine(".NET Fallback Language/Locale:" + netLanguage + " (application-specific)");
			return netLanguage;
		}
	}
}
