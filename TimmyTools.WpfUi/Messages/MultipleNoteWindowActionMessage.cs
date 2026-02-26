using TimmyTools.Core.Enums;

namespace TimmyTools.WpfUi.Messages;

public record MultipleNoteWindowActionMessage(
    IEnumerable<int> NoteIds,
    NoteWindowAction Action
);
