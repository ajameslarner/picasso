using System.Globalization;
using System.Windows.Data;

namespace Picasso.Utilities.Converters;

public class HeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double height)
        {
            return height - 500;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
