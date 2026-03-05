using System.Windows;

namespace TimmyTools.WpfUi.Messages;

public record OpenSettingsWindowMessage(
    Window? Owner = null,
    int TabIndex = 0
);
