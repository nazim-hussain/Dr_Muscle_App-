using System;
using System.Globalization;
using System.Threading;
using Foundation;
using DrMuscle.Localize;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(DrMuscle.iOS.Localize))]

namespace DrMuscle.iOS
{
	public class Localize : ILocalize
	{
		public Localize()
		{
            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") == null)
            {
                LocalDBManager.Instance.SetDBSetting("AppLanguage", "en");
            }
            NSArray languageArray = NSArray.FromObjects(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value);
			NSUserDefaults.StandardUserDefaults.SetValueForKey(languageArray, (Foundation.NSString)"AppleLanguages");
			//System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.AppLanguageCode);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value);
		}
		public void SetLocale(CultureInfo ci)
		{
            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") == null)
            {
                LocalDBManager.Instance.SetDBSetting("AppLanguage", "en");
            }

            //Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
			Console.WriteLine("CurrentCulture set: " + ci.Name);
		}

		public CultureInfo GetCurrentCultureInfo()
		{
			var netLanguage = "en";
			//Config.AppLanguageId
			if (NSLocale.PreferredLanguages.Length > 0)
			{
				var pref = NSLocale.PreferredLanguages[0];
				netLanguage = iOSToDotnetLanguage(pref);
			}
			NSUserDefaults.StandardUserDefaults.SetString("ar", "AppLanguages");
			NSUserDefaults.StandardUserDefaults.Synchronize();

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

		string iOSToDotnetLanguage(string iOSLanguage)
		{
			Console.WriteLine("iOS Language:" + iOSLanguage);
			var netLanguage = iOSLanguage;

			//certain languages need to be converted to CultureInfo equivalent
			switch (iOSLanguage)
			{
				case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
				case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
					netLanguage = "ms"; // closest supported
					break;
				case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
					netLanguage = "de-CH"; // closest supported
					break;
					// add more application-specific cases here (if required)
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

				case "pt":
					netLanguage = "pt-PT"; // fallback to Portuguese (Portugal)
					break;
				case "gsw":
					netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
					break;
				case "fr":
					netLanguage = "fr"; // equivalent to French
					break;

			}

			Console.WriteLine(".NET Fallback Language/Locale:" + netLanguage + " (application-specific)");
			return netLanguage;
		}
	}
}
