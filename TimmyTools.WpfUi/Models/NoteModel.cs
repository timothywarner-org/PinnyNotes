using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using TimmyTools.Core.DataTransferObjects;
using TimmyTools.Core.Enums;
using TimmyTools.WpfUi.Themes;

namespace TimmyTools.WpfUi.Models;

public class NoteModel : BaseModel
{
    public NoteModel(NoteSettingsModel NoteSettings, string themeColourScheme)
    {

        Width = NoteSettings.DefaultWidth;
        Height = NoteSettings.DefaultHeight;

        ThemeColourScheme = themeColourScheme;

        TransparencyEnabled = (NoteSettings.TransparencyMode != TransparencyMode.Disabled);
    }

    public NoteModel(NoteDto noteDto, NoteSettingsModel NoteSettings)
    {
        Id = noteDto.Id;

        Content = noteDto.Content;

        X = noteDto.X;
        Y = noteDto.Y;
        Width = noteDto.Width;
        Height = noteDto.Height;

        GravityX = noteDto.GravityX;
        GravityY = noteDto.GravityY;

        ThemeColourScheme = noteDto.ThemeColourScheme;

        IsOpen = noteDto.IsOpen;
        Title = noteDto.Title;

        TransparencyEnabled = (NoteSettings.TransparencyMode != TransparencyMode.Disabled);
    }

    public int Id { get; set => SetProperty(ref field, value); }

    public string Content { get; set => SetProperty(ref field, value); } = "";

    public double X { get; set => SetProperty(ref field, value); }
    public double Y { get; set => SetProperty(ref field, value); }

    public double Width { get; set => SetProperty(ref field, value); }
    public double Height { get; set => SetProperty(ref field, value); }

    public int GravityX { get; set => SetProperty(ref field, value); }
    public int GravityY { get; set => SetProperty(ref field, value); }

    public string ThemeColourScheme { get; set => SetProperty(ref field, value); }

    public bool IsOpen { get; set => SetProperty(ref field, value); }
    public string? Title { get; set => SetProperty(ref field, value); }


    public nint WindowHandle { get; set; }

    public bool IsFocused { get; set => SetProperty(ref field, value); }

    public bool TransparencyEnabled { get; set => SetProperty(ref field, value); }
    public double Opacity { get; set => SetProperty(ref field, value); }
    public bool ShowInTaskbar { get; set => SetProperty(ref field, value); }

    public Brush Background { get; set => SetProperty(ref field, value); } = Brushes.LightGray;
    public Brush BorderBrush { get; set => SetProperty(ref field, value); } = Brushes.DarkGray;
    public Brush TitleGridBackground { get; set => SetProperty(ref field, value); } = Brushes.Gray;
    public Brush TitleGridButtonForeground { get; set => SetProperty(ref field, value); } = Brushes.DarkGray;
    public Brush ContentTextBoxForeground { get; set => SetProperty(ref field, value); } = Brushes.Black;


    public void UpdateBrushes(Palette palette)
    {
        Background = new SolidColorBrush(palette.Background);
        BorderBrush = new SolidColorBrush(palette.Border);
        TitleGridBackground = new SolidColorBrush(palette.Title);
        TitleGridButtonForeground = new SolidColorBrush(palette.Button);
        ContentTextBoxForeground = new SolidColorBrush(palette.Text);
    }

    public NoteDto ToDto()
    {
        NoteDto noteDto = new(
            Id,

            Content,

            X,
            Y,
            Width,
            Height,

            GravityX,
            GravityY,

            ThemeColourScheme,

            IsOpen,
            Title
        );

        return noteDto;
    }

    public static string GetPlainTextFromContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return "";

        if (!content.TrimStart().StartsWith(@"{\rtf"))
            return content;

        try
        {
            FlowDocument doc = new();
            TextRange range = new(doc.ContentStart, doc.ContentEnd);
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(content));
            range.Load(stream, DataFormats.Rtf);

            string text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
            if (text.EndsWith("\r\n"))
                text = text[..^2];
            return text;
        }
        catch (Exception ex)
        {
            // If RTF parsing fails, return the raw content to prevent accidental deletion
            System.Diagnostics.Debug.WriteLine($"Failed to parse RTF content: {ex.Message}");
            return content;
        }
    }
}
