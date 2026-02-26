using System.Security.Cryptography;
using System.Text;

using PinnyNotes.Core.Enums;
using PinnyNotes.WpfUi.Commands;
using PinnyNotes.WpfUi.Controls;

namespace PinnyNotes.WpfUi.Tools;

public class HashTool : BaseTool, ITool
{
    private enum ToolActions
    {
        HashSHA512,
        HashSHA384,
        HashSHA256,
        HashSHA1,
        HashMD5
    }

    public HashTool(NoteTextBoxControl noteTextBox) : base(noteTextBox)
    {
        InitializeMenuItem(
            "Hash",
            [
                new ToolMenuAction("SHA512", new RelayCommand(() => MenuAction(ToolActions.HashSHA512))),
                new ToolMenuAction("SHA384", new RelayCommand(() => MenuAction(ToolActions.HashSHA384))),
                new ToolMenuAction("SHA256", new RelayCommand(() => MenuAction(ToolActions.HashSHA256))),
                new ToolMenuAction("SHA1", new RelayCommand(() => MenuAction(ToolActions.HashSHA1))),
                new ToolMenuAction("MD5", new RelayCommand(() => MenuAction(ToolActions.HashMD5)))
            ]
        );
    }

    public ToolState State => ToolSettings.HashToolState;

    private void MenuAction(ToolActions action)
    {
        ApplyFunctionToNoteText(ModifyTextCallback, action);
    }

    private string ModifyTextCallback(string text, Enum action)
    {
        using HashAlgorithm hasher = action switch
        {
            ToolActions.HashSHA512 => SHA512.Create(),
            ToolActions.HashSHA384 => SHA384.Create(),
            ToolActions.HashSHA256 => SHA256.Create(),
            ToolActions.HashSHA1 => SHA1.Create(),
            ToolActions.HashMD5 => MD5.Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        return Convert.ToHexString(
            hasher.ComputeHash(
                Encoding.UTF8.GetBytes(text)
            )
        );
    }
}
