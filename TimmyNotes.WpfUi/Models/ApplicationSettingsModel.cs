namespace TimmyNotes.WpfUi.Models;

public class ApplicationSettingsModel : BaseModel
{
    // General
    public bool ShowNotifyIcon { get; set => SetProperty(ref field, value); }
    public bool CheckForUpdates { get; set => SetProperty(ref field, value); }
}
