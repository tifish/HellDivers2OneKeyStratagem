using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Helldivers2OneKeyStratagem;

public class RadioButtonToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string currentStringValue && parameter is string stringValue)
            return currentStringValue == stringValue;

        return BindingOperations.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked && parameter is string stringValue)
            if (isChecked)
                return stringValue;

        return BindingOperations.DoNothing;
    }
}
