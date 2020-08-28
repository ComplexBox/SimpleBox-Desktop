using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleBox.Utils
{
    public class SingleLineTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return string.Empty;
            string s = (string) value;
            s = s.Replace(Environment.NewLine, " ");
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
