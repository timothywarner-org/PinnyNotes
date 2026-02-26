using TimmyNotes.Core.DataTransferObjects;
using TimmyNotes.Core.Enums;

namespace TimmyNotes.WpfUi.Messages;

public record NoteActionMessage(
    NoteAction Action,
    NoteDto NoteDto
);
