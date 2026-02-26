using TimmyNotes.Core.Enums;
using TimmyNotes.WpfUi.Commands;
using TimmyNotes.WpfUi.Controls;

namespace TimmyNotes.WpfUi.Tools;

public class DateTimeTool : BaseTool, ITool
{
    private enum ToolActions
    {
        DateTimeSortableDateTime
    }

    public DateTimeTool(NoteTextBoxControl noteTextBox) : base(noteTextBox)
    {
        InitializeMenuItem(
            "Date Time",
            [
                new ToolMenuAction("Sortable Date Time", new RelayCommand(() => MenuAction(ToolActions.DateTimeSortableDateTime)))
            ]
        );
    }

    public ToolState State => ToolSettings.DateTimeToolState;

    private void MenuAction(ToolActions action)
    {
        switch (action)
        {
            case ToolActions.DateTimeSortableDateTime:
                InsertIntoNoteText(GetSortableDateTime());
                break;
        }
    }

    private string GetSortableDateTime()
    {
        string selectedText = NoteTextBox.Selection.Text;
        return GetDateTime("s", selectedText);
    }

    private static string GetDateTime(string format, string? dateString = null)
    {
        if (string.IsNullOrEmpty(dateString))
            return DateTime.UtcNow.ToString(format);

        if (DateTime.TryParse(dateString, out DateTime parsedDateTime))
            return parsedDateTime.ToString(format);

        return string.Empty;
    }
}
