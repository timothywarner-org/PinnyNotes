using TimmyTools.WpfUi.Models;

namespace TimmyTools.WpfUi.Messages;

public record OpenNoteWindowMessage(
    int? NoteId = null,
    NoteModel? ParentNote = null,
    bool isManagementWindowParent = false
);
