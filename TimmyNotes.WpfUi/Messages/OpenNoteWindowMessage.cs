using TimmyNotes.WpfUi.Models;

namespace TimmyNotes.WpfUi.Messages;

public record OpenNoteWindowMessage(
    int? NoteId = null,
    NoteModel? ParentNote = null,
    bool isManagementWindowParent = false
);
