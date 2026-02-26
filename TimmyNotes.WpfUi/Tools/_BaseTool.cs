using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

using TimmyNotes.WpfUi.Controls;
using TimmyNotes.WpfUi.Services;
using TimmyNotes.WpfUi.Models;

namespace TimmyNotes.WpfUi.Tools;

public abstract class BaseTool
{
    protected ToolSettingsModel ToolSettings;

    protected NoteTextBoxControl NoteTextBox;

    private MenuItem? _menuItem;

    public BaseTool(NoteTextBoxControl noteTextBox)
    {
        NoteTextBox = noteTextBox;

        SettingsService settingsService = App.Services.GetRequiredService<SettingsService>();
        ToolSettings = settingsService.ToolSettings;
    }

    public MenuItem MenuItem => _menuItem
        ?? throw new Exception("Tools menu has not been initialised.");

    protected void InitializeMenuItem(string header, ToolMenuAction[] menuActions)
    {
        _menuItem = new()
        {
            Header = header,
        };

        foreach (ToolMenuAction menuAction in menuActions)
        {
            if (menuAction.Name == "-" && menuAction.Command is null && menuAction.Action is null)
            {
                _menuItem.Items.Add(new Separator());
                continue;
            }

            MenuItem actionMenuItem = new()
            {
                Header = menuAction.Name
            };
            if (menuAction.Command != null)
                actionMenuItem.Command = menuAction.Command;
            if (menuAction.Action != null)
                actionMenuItem.CommandParameter = menuAction.Action;

            _menuItem.Items.Add(actionMenuItem);
        }
    }

    protected void ApplyFunctionToNoteText(Func<string, Enum, string> function, Enum action)
    {
        if (NoteTextBox.HasSelectedText)
        {
            NoteTextBox.Selection.Text = function(NoteTextBox.Selection.Text, action);
        }
        else
        {
            string noteText = NoteTextBox.GetPlainText();
            NoteTextBox.SelectAll();
            NoteTextBox.Selection.Text = function(noteText, action);
        }
    }

    protected void ApplyFunctionToEachLine(Func<string, int, Enum, string?> function, Enum action)
    {
        string noteText = NoteTextBox.HasSelectedText ? NoteTextBox.Selection.Text : NoteTextBox.GetPlainText();

        string[] lines = noteText.Split(Environment.NewLine);

        List<string> newLines = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string? line = function(lines[i], i, action);
            if (line != null)
                newLines.Add(line);
        }

        noteText = string.Join(Environment.NewLine, newLines);

        if (NoteTextBox.HasSelectedText)
        {
            NoteTextBox.Selection.Text = noteText;
        }
        else
        {
            NoteTextBox.SelectAll();
            NoteTextBox.Selection.Text = noteText;
        }
    }

    protected void InsertIntoNoteText(string text)
    {
        NoteTextBox.Selection.Text = text;

        if (NoteTextBox.KeepNewLineAtEndVisible)
            NoteTextBox.ScrollToEnd();
    }
}
