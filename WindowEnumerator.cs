using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace DevelopingInsanity.KeyMacro.Macros;

public class WindowEnumerator: IEnumerable<string>
{ 
    private List<string> _windowTitles = [];

    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= _windowTitles.Count)
                throw new IndexOutOfRangeException("Index is out of range.");
            return _windowTitles[index];
        }
    }

    public int Count => _windowTitles.Count;

    internal WindowEnumerator()
    { }

    public static WindowEnumerator Create()
    {
        WindowEnumerator enumerator = new WindowEnumerator();
        if (!Win32Native.EnumWindows((hWnd, lParam) =>
        {
            bool result = true;

            if (Win32Native.IsWindowVisible(hWnd))
            {
                int length = Win32Native.GetWindowTextLength(hWnd);
                if (length > 0)
                {
                    StringBuilder builder = new(length + 1);
                    if (Win32Native.GetWindowText(hWnd, builder, builder.Capacity) <= 0)
                    {
                        Marshal.SetLastSystemError(Marshal.GetLastWin32Error());
                        result = false;
                    }
                    else
                        enumerator._windowTitles.Add(builder.ToString());
                }
            }
            return result;
        }, IntPtr.Zero))
            throw new IOException($"Failed to enumerate windows ({Marshal.GetLastWin32Error()})");
        return enumerator;
    }

    public IEnumerator<string> GetEnumerator()
    {
        return _windowTitles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
