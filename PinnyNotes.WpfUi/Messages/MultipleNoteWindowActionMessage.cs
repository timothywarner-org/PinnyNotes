using PinnyNotes.Core.Enums;

namespace PinnyNotes.WpfUi.Messages;

public record MultipleNoteWindowActionMessage(
    IEnumerable<int> NoteIds,
    NoteWindowAction Action
);
