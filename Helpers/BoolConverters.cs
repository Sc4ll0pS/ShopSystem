using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShopSystem.Helpers
{
    /// <summary>Converts bool to one of two strings split by '|'</summary>
    public class BoolToStringConverter : IValueConverter
    {
        public static readonly BoolToStringConverter Instance = new();
        public object Convert(object value, Type t, object param, CultureInfo c)
        {
            var parts = param?.ToString()?.Split('|') ?? new[] { "True", "False" };
            return (bool)value ? parts[0] : parts[1];
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    /// <summary>True -> Visible, False -> Collapsed</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public static readonly BoolToVisibilityConverter TrueVisible  = new() { Invert = false };
        public static readonly BoolToVisibilityConverter FalseVisible = new() { Invert = true };

        public bool Invert { get; set; }

        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            bool b = (bool)value;
            if (Invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
