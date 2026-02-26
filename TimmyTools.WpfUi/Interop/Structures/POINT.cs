using System.Runtime.InteropServices;

namespace TimmyTools.WpfUi.Interop.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int x;
    public int y;
}
