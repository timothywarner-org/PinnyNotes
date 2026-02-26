using System.Globalization;
using System.Windows;
using System.Windows.Data;

using TimmyNotes.WpfUi.ViewModels;

namespace TimmyNotes.WpfUi.Views;

public partial class BreakTimerWindow : Window
{
    public BreakTimerWindow(BreakTimerViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}

public class FractionToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2
            && values[0] is double fraction
            && values[1] is double totalWidth)
        {
            return Math.Max(0, fraction * totalWidth);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
