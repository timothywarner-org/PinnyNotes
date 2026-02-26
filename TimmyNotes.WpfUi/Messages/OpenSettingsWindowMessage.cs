using System.Windows;

namespace TimmyNotes.WpfUi.Messages;

public record OpenSettingsWindowMessage(
    Window? Owner = null
);
