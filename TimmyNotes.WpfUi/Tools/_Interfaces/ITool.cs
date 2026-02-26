using System.Windows.Controls;

using PinnyNotes.Core.Enums;

namespace PinnyNotes.WpfUi.Tools;

public interface ITool
{
    ToolState State { get; }

    MenuItem MenuItem { get; }
}
