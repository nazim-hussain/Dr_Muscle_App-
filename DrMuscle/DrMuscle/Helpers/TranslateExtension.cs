using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using DrMuscle.Helpers;
using DrMuscle.Localize;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Resx
{
	[ContentProperty("Text")]
	public class TranslateExtension : IMarkupExtension
	{
		readonly CultureInfo ci = null;
		const string ResourceId = "DrMuscle.Resx.AppResources";

		public TranslateExtension()
		{
			if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android)
			{
                try
                {

				    ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();

                }
                catch (Exception ex)
                {

                }
            }
		}

		public string Text { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (Text == null)
				return "";
            try
            {

            ResourceManager resourceManager = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
            if (ResourceLoader.Instance == null)
                new ResourceLoader(resourceManager);
            var translation = resourceManager.GetString(Text, ci);

			return translation;

            }
            catch (Exception ex)
            {
                return "";
            }
        }
	}
}
