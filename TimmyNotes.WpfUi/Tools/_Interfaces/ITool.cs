using System.Windows.Controls;

using TimmyNotes.Core.Enums;

namespace TimmyNotes.WpfUi.Tools;

public interface ITool
{
    ToolState State { get; }

    MenuItem MenuItem { get; }
}
