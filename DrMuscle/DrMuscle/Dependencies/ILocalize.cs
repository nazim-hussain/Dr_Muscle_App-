using System;
using System.Globalization;

namespace DrMuscle.Localize
{
	public interface ILocalize
	{
		CultureInfo GetCurrentCultureInfo();
		void SetLocale(CultureInfo ci);
	}
}
