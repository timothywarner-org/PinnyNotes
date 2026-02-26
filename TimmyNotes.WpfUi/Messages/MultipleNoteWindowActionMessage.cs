using TimmyNotes.Core.Enums;

namespace TimmyNotes.WpfUi.Messages;

public record MultipleNoteWindowActionMessage(
    IEnumerable<int> NoteIds,
    NoteWindowAction Action
);
