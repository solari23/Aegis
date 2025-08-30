using System.Runtime.InteropServices;

namespace Aegis.Passkeys.Windows.Structures;

[StructLayout(LayoutKind.Sequential)]
internal struct HWnd
{
    private IntPtr handle;

    public HWnd(IntPtr handle) => this.handle = handle;
}
