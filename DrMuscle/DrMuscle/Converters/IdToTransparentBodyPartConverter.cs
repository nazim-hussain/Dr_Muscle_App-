using System;
using System.Globalization;
using Xamarin.Forms;
using DrMuscle.Constants;
namespace DrMuscle.Converters
{
    public class IdToTransparentBodyPartConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return "Undefined_Transparent.png";
                var content = (long)value;
                if (content == 14)
                    return "Lower_back_Transparent.png";
                if (content == 13)
                    return "Flexibility_Transparent.png";
                return $"{AppThemeConstants.GetBodyPartName(content)}_Transparent.png";
            }
            catch (Exception ex)
            {

            }
            return "";
            
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
