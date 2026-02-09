using PinnyNotes.Core.DataTransferObjects;
using PinnyNotes.Core.Enums;

namespace PinnyNotes.WpfUi.Messages;

public record NoteActionMessage(
    NoteAction Action,
    NoteDto NoteDto
);
