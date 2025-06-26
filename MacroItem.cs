using System.Runtime.InteropServices;

namespace DevelopingInsanity.KeyMacro.Macros;

public enum VirtualKey : int
{
    // Mouse buttons
    LeftButton = 0x01,
    RightButton = 0x02,
    MiddleButton = 0x04,
    XButton1 = 0x05,
    XButton2 = 0x06,

    // Control keys
    Back = 0x08,
    Tab = 0x09,
    Enter = 0x0D,
    Shift = 0x10,
    Control = 0x11,
    Alt = 0x12,
    Pause = 0x13,
    CapsLock = 0x14,
    Escape = 0x1B,
    Space = 0x20,

    // Navigation keys
    PageUp = 0x21,
    PageDown = 0x22,
    End = 0x23,
    Home = 0x24,
    LeftArrow = 0x25,
    UpArrow = 0x26,
    RightArrow = 0x27,
    DownArrow = 0x28,
    Insert = 0x2D,
    Delete = 0x2E,

    // Number keys
    D0 = 0x30,
    D1 = 0x31,
    D2 = 0x32,
    D3 = 0x33,
    D4 = 0x34,
    D5 = 0x35,
    D6 = 0x36,
    D7 = 0x37,
    D8 = 0x38,
    D9 = 0x39,

    // Alphabet keys
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,

    // Function keys
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,

    // Numpad keys
    NumPad0 = 0x60,
    NumPad1 = 0x61,
    NumPad2 = 0x62,
    NumPad3 = 0x63,
    NumPad4 = 0x64,
    NumPad5 = 0x65,
    NumPad6 = 0x66,
    NumPad7 = 0x67,
    NumPad8 = 0x68,
    NumPad9 = 0x69,
    Multiply = 0x6A,
    Add = 0x6B,
    Subtract = 0x6D,
    Decimal = 0x6E,
    Divide = 0x6F,

    // Additional common keys
    Semicolon = 0xBA,        // ';:'
    Equal = 0xBB,            // '=+'
    Comma = 0xBC,            // ',<'
    Minus = 0xBD,            // '-_'
    Period = 0xBE,           // '.>'
    Slash = 0xBF,            // '/?'
    Grave = 0xC0,            // '`~'
    LeftBracket = 0xDB,      // '[{'
    Backslash = 0xDC,        // '\|'
    RightBracket = 0xDD,     // ']}'
    Quote = 0xDE             // ''"'
}

internal partial class Win32Native
{
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


    public static nint GetVirtualKeyCode(string key)
    {
        // Handle common key name variations
        var normalizedKey = key.ToLowerInvariant() switch
        {
            "enter" or "return" => "Enter",
            "space" or "spacebar" => "Space",
            "ctrl" or "control" => "Control",
            "alt" => "Alt",
            "shift" => "Shift",
            "esc" or "escape" => "Escape",
            "tab" => "Tab",
            "backspace" or "back" => "Back",
            "delete" or "del" => "Delete",
            "insert" or "ins" => "Insert",
            "home" => "Home",
            "end" => "End",
            "pageup" or "pgup" => "PageUp",
            "pagedown" or "pgdn" => "PageDown",
            "leftarrow" or "left" => "LeftArrow",
            "rightarrow" or "right" => "RightArrow",
            "uparrow" or "up" => "UpArrow",
            "downarrow" or "down" => "DownArrow",
            "0" => "D0",
            "1" => "D1",
            "2" => "D2",
            "3" => "D3",
            "4" => "D4",
            "5" => "D5",
            "6" => "D6",
            "7" => "D7",
            "8" => "D8",
            "9" => "D9",
            _ => key
        };

        if (Enum.TryParse<VirtualKey>(normalizedKey, true, out var vk))
        {
            return (nint)vk;
        }

        // Try direct parsing for single characters
        if (normalizedKey.Length == 1)
        {
            char c = char.ToUpperInvariant(normalizedKey[0]);
            if (c >= 'A' && c <= 'Z')
            {
                return (ushort)c;
            }
            if (c >= '0' && c <= '9')
            {
                return (ushort)c;
            }
        }

        return 0;
    }
}

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
    protected override bool OnKeyExecute()
    {
        if (!Win32Native.PostMessage(_hWnd, Win32Native.WM_KEYDOWN, _keyTarget, IntPtr.Zero))
            return false;
        Thread.Sleep(50); // Simulate a short delay
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
        return $"{Action} {Milliseconds}ms";
    }
}