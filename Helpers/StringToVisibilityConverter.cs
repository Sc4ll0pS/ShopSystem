using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShopSystem.Helpers
{
    /// <summary>Hides a control when the bound string is null/empty.</summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public static readonly StringToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hide = parameter?.ToString() == "hide";
            bool empty = string.IsNullOrEmpty(value?.ToString());
            if (hide) return empty ? Visibility.Collapsed : Visibility.Visible;
            return empty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
