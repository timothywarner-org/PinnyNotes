using PinnyNotes.Core.Enums;
using PinnyNotes.WpfUi.Commands;
using PinnyNotes.WpfUi.Controls;

namespace PinnyNotes.WpfUi.Tools;

public class GuidTool : BaseTool, ITool
{
    private enum ToolActions
    {
        GuidV4,
        GuidV7
    }

    public GuidTool(NoteTextBoxControl noteTextBox) : base(noteTextBox)
    {
        InitializeMenuItem(
            "GUID",
            [
                new ToolMenuAction("Version 4 - Random", new RelayCommand(() => MenuAction(ToolActions.GuidV4))),
                new ToolMenuAction("Version 7 - Sequential", new RelayCommand(() => MenuAction(ToolActions.GuidV7)))
            ]
        );
    }

    public ToolState State => ToolSettings.GuidToolState;

    private void MenuAction(ToolActions action)
    {
        Guid newGuid;
        switch (action)
        {
            case ToolActions.GuidV4:
                newGuid = Guid.NewGuid();
                break;
            case ToolActions.GuidV7:
                newGuid = Guid.CreateVersion7();
                break;
            default:
                return;
        }

        InsertIntoNoteText(newGuid.ToString().ToUpper());
    }
}
