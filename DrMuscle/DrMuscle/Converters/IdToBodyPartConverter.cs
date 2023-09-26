using System;
using System.Globalization;
using Xamarin.Forms;
using DrMuscle.Constants;
namespace DrMuscle.Converters
{
    public class IdToBodyPartConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return "Undefined.png";
                var content = (long)value;
                if (content == 14)
                    return "Lower_back.png";
                if (content == 13 || content == 28)
                    return "Flexibility.png";
                
                return $"{AppThemeConstants.GetBodyPartName(content)}.png".Replace(" ","_");
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
