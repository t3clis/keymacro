using System.Runtime.InteropServices;
using System.Text;

namespace DevelopingInsanity.KeyMacro.Macros;



public enum MacroAction
    {
        SendKey,
        PressKey,
        ReleaseKey,
        Wait
    }

public abstract class MacroItem(MacroAction action)
{
    public MacroAction Action { get; private set; } = action;
    

    public int LastError { get; protected set; } = 0;

    protected virtual bool OnExecute()
    {
        return false;
    }
    public bool Execute()
    {
        return OnExecute();
    }

    public override string ToString()
    {
        return Action.ToString();
    }
}

public abstract class KeyMacroItem : MacroItem
{
    protected nint _hWnd;
    protected nint _keyTarget;
    public string TargetWindow { get; private set; }

    public string Key { get; private set; }

    public KeyMacroItem(MacroAction action, string key, string targetWindow): base(action)
    {
        TargetWindow = targetWindow;
        Key = key;
        _hWnd = Win32Native.FindWindow(null, targetWindow);
        if (_hWnd == IntPtr.Zero)
        {
            LastError = Marshal.GetLastWin32Error();
        }
        _keyTarget = Win32Native.GetVirtualKeyCode(key);
        if (_keyTarget == 0)
        {
            LastError = Marshal.GetLastWin32Error();
        }
    }


    protected override bool OnExecute()
    {
        if(_hWnd == IntPtr.Zero) return false;
        if (_keyTarget == 0) return false;
        if (!Win32Native.SetForegroundWindow(_hWnd))
        {
            LastError = Marshal.GetLastWin32Error();
            return false;
        }

        return OnKeyExecute();
    }

    protected virtual bool OnKeyExecute()
    {
        // This method should be overridden in derived classes to implement specific key actions
        return false;
    }

    public override string ToString()
    {
        return $"{Action} {Key}";
    }
}

public class SendKeyMacro(string window, string key) : KeyMacroItem(MacroAction.SendKey, key, window)
{
    private readonly Random _r = new(DateTime.UtcNow.Millisecond);

    protected override bool OnKeyExecute()
    {

        if (!Win32Native.PostMessage(_hWnd, Win32Native.WM_KEYDOWN, _keyTarget, IntPtr.Zero))
            return false;
        Thread.Sleep(_r.Next(70, 130)); // Simulate a short delay
        return Win32Native.PostMessage(_hWnd, Win32Native.WM_KEYUP, _keyTarget, IntPtr.Zero);
    }
}

public class PressKeyMacro(string window, string key) : KeyMacroItem(MacroAction.PressKey, key, window)
{

    protected override bool OnKeyExecute()
    {
       return !Win32Native.PostMessage(_hWnd, Win32Native.WM_KEYDOWN, _keyTarget, IntPtr.Zero);
    }
}

public class ReleaseKeyMacro(string window, string key) : KeyMacroItem(MacroAction.ReleaseKey, key, window)
{
    protected override bool OnKeyExecute()
    {
        return !Win32Native.PostMessage(_hWnd, Win32Native.WM_KEYUP, _keyTarget, IntPtr.Zero);
    }
}

public class WaitMacro(int milliseconds) : MacroItem(MacroAction.Wait)
{
    public int Milliseconds { get; private set; } = milliseconds;

    protected override bool OnExecute()
    {
        Thread.Sleep(Milliseconds);
        LastError = 0; // Reset last error on successful wait
        return true;
    }

    public override string ToString()
    {
        return $"{Action} {Milliseconds}";
    }
}