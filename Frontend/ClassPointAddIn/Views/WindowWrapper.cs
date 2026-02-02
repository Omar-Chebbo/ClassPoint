using System;
using System.Windows.Forms;

public class WindowWrapper : IWin32Window
{
    private readonly IntPtr _hwnd;
    public WindowWrapper(IntPtr handle)
    {
        _hwnd = handle;
    }

    public IntPtr Handle => _hwnd;
}
