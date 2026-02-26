using TimmyTools.Core.DataTransferObjects;
using TimmyTools.Core.Enums;

namespace TimmyTools.WpfUi.Messages;

public record NoteActionMessage(
    NoteAction Action,
    NoteDto NoteDto
);
