using System.Windows;

using TimmyTools.WpfUi.Messages;
using TimmyTools.WpfUi.Services;
using TimmyTools.WpfUi.ViewModels;

namespace TimmyTools.WpfUi.Views;

public partial class BreakTimerWindow : Window
{
    private readonly MessengerService _messengerService;

    public BreakTimerWindow(MessengerService messengerService, BreakTimerViewModel viewModel)
    {
        _messengerService = messengerService;
        DataContext = viewModel;
        InitializeComponent();

        Closed += (s, e) =>
        {
            if (DataContext is IDisposable disposable)
                disposable.Dispose();
        };
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _messengerService.Publish(new OpenSettingsWindowMessage(TabIndex: 3));
    }
}
