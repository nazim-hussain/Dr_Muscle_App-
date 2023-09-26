using System;
using System.Globalization;
using Xamarin.Forms;

namespace DrMuscle.Converters
{
    public class InttoBoolConvertor : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var content = (int)value;
            if (content == -1)
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
