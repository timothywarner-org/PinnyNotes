using PinnyNotes.Core.DataTransferObjects;
using PinnyNotes.Core.Enums;

namespace PinnyNotes.WpfUi.Messages;

public record NoteActionMessage(
    NoteActions Action,
    NoteDto NoteDto
);
