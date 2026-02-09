using PinnyNotes.Core.Enums;
using PinnyNotes.WpfUi.Helpers;
using PinnyNotes.WpfUi.Themes;

namespace PinnyNotes.WpfUi.Services;

public class ThemeService(AppMetadataService appMetadataService, SettingsService settingsService)
{
    private readonly AppMetadataService _appMetadataService = appMetadataService;
    private readonly SettingsService _settingsService = settingsService;

    public Theme CurrentTheme { get; } = new DefaultTheme();

    public string GetNewNoteColorSchemeName(string? parentColorScheme = null)
    {
        // Set this first as cycle colors wont trigger a change if the next color if the default for ThemeColors
        string currentColorScheme = _appMetadataService.Metadata.ColorScheme ?? CurrentTheme.DefaultColorSchemeName;
        if (_settingsService.NoteSettings.CycleColors)
            currentColorScheme = GetNextColorSchemeName(currentColorScheme, parentColorScheme);

        return currentColorScheme;
    }

    public string GetNextColorSchemeName(string currentColorSchemeName, string? parentColorSchemeName = null)
    {
        string[] keys = [.. CurrentTheme.ColorSchemes.Keys];

        int index = keys.IndexOf(currentColorSchemeName) + 1;

        string nextName = (index == keys.Length) ? keys[0] : keys[index];
        if (nextName == parentColorSchemeName)
        {
            index++;
            nextName = (index == keys.Length) ? keys[0] : keys[index];
        }

        return nextName;
    }

    public Palette GetPalette(string colorSchemeName, ColorModes colorMode)
    {
        ColorScheme colorScheme = CurrentTheme.ColorSchemes[colorSchemeName];

        Palette palette;
        if (colorMode == ColorModes.Dark || (colorMode == ColorModes.System && SystemThemeHelper.IsDarkMode()))
            palette = colorScheme.Dark;
        else
            palette = colorScheme.Light;

        return palette;
    }
}
