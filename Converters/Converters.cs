using System.Globalization;

namespace BuscaMinasDari.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRevealed = (bool)value;

            if (parameter?.ToString() == "BackgroundColor")
            {
                if (isRevealed)
                {
                    return Application.Current.RequestedTheme == AppTheme.Dark
                        ? Application.Current.Resources["CellRevealedDark"]
                        : Application.Current.Resources["CellRevealed"];
                }
                else
                {
                    return Application.Current.RequestedTheme == AppTheme.Dark
                        ? Application.Current.Resources["CellUnrevealedDark"]
                        : Application.Current.Resources["CellUnrevealed"];
                }
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NumberToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return number switch
                {
                    1 => Application.Current.Resources["Number1"],
                    2 => Application.Current.Resources["Number2"],
                    3 => Application.Current.Resources["Number3"],
                    4 => Application.Current.Resources["Number4"],
                    5 => Application.Current.Resources["Number5"],
                    6 => Application.Current.Resources["Number6"],
                    7 => Application.Current.Resources["Number7"],
                    8 => Application.Current.Resources["Number8"],
                    _ => Colors.Black
                };
            }

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}