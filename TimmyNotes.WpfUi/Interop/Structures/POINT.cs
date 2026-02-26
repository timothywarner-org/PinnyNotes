using System.Runtime.InteropServices;

namespace TimmyNotes.WpfUi.Interop.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int x;
    public int y;
}
