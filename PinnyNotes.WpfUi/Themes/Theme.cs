namespace PinnyNotes.WpfUi.Themes;

public abstract class Theme
{
    public abstract string Name { get; }
    public abstract Dictionary<string, ColorScheme> ColorSchemes { get; }
    public abstract string DefaultColorSchemeName { get; }
}
