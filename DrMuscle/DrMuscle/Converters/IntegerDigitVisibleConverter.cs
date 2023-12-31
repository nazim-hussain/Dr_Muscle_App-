﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace DrMuscle.Convertors
{
	internal class IntegerDigitVisibleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var intDigitLength = (int)value;
			var digitIndex = System.Convert.ToInt32(parameter);

			return digitIndex < intDigitLength;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
