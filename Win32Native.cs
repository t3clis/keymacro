
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace DevelopingInsanity.KeyMacro.Macros;

[StructLayout(LayoutKind.Sequential)]
internal struct MSG
{
    public IntPtr hwnd;
    public uint message;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public POINT pt;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int x;
    public int y;
}

public enum KeyOp
{
    None,
    Up,
    Down
}

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
    public delegate bool EnumWindowsProc(nint hWnd, nint lParam);
    public delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYUP = 0x0104;
    public const int WM_SYSKEYDOWN = 0x0105;
    public const int WH_KEYBOARD_LL = 13;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);



    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    public static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll")]
    public static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(nint hhk,
        int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll")]
    public static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern bool DispatchMessage(ref MSG lpMsg);


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

    internal static ConsoleKey? ToConsoleKey(int virtualKey)
    {
        if (virtualKey == 0)
            return null;

        if (!Enum.IsDefined(typeof(VirtualKey), virtualKey))
            return null;

        VirtualKey vk = (VirtualKey)virtualKey;
        return vk switch
        {
            VirtualKey.Enter => ConsoleKey.Enter,
            VirtualKey.Space => ConsoleKey.Spacebar,
            VirtualKey.Escape => ConsoleKey.Escape,
            VirtualKey.Tab => ConsoleKey.Tab,
            VirtualKey.Back => ConsoleKey.Backspace,
            VirtualKey.Delete => ConsoleKey.Delete,
            VirtualKey.Insert => ConsoleKey.Insert,
            VirtualKey.Home => ConsoleKey.Home,
            VirtualKey.End => ConsoleKey.End,
            VirtualKey.PageUp => ConsoleKey.PageUp,
            VirtualKey.PageDown => ConsoleKey.PageDown,
            VirtualKey.LeftArrow => ConsoleKey.LeftArrow,
            VirtualKey.RightArrow => ConsoleKey.RightArrow,
            VirtualKey.UpArrow => ConsoleKey.UpArrow,
            VirtualKey.DownArrow => ConsoleKey.DownArrow,
            VirtualKey.F1 => ConsoleKey.F1,
            VirtualKey.F2 => ConsoleKey.F2,
            VirtualKey.F3 => ConsoleKey.F3,
            VirtualKey.F4 => ConsoleKey.F4,
            VirtualKey.F5 => ConsoleKey.F5,
            VirtualKey.F6 => ConsoleKey.F6,
            VirtualKey.F7 => ConsoleKey.F7,
            VirtualKey.F8 => ConsoleKey.F8,
            VirtualKey.F9 => ConsoleKey.F9,
            VirtualKey.F10 => ConsoleKey.F10,
            VirtualKey.F11 => ConsoleKey.F11,
            VirtualKey.F12 => ConsoleKey.F12,
            _ when (virtualKey >= (int)VirtualKey.D0 && virtualKey <= (int)VirtualKey.D9) =>
                (ConsoleKey)(virtualKey - (int)VirtualKey.D0 + (int)ConsoleKey.D0),
            _ when (virtualKey >= (int)VirtualKey.A && virtualKey <= (int)VirtualKey.Z) =>
                (ConsoleKey)(virtualKey - (int)VirtualKey.A + (int)ConsoleKey.A),
            _ => null
        };
    }

    internal static int ToVirtualKeyCode(ConsoleKey? key)
    {
        if (key == null)
            return 0;

        return key switch
        {
            ConsoleKey.Enter => (int)VirtualKey.Enter,
            ConsoleKey.Spacebar => (int)VirtualKey.Space,
            ConsoleKey.Escape => (int)VirtualKey.Escape,
            ConsoleKey.Tab => (int)VirtualKey.Tab,
            ConsoleKey.Backspace => (int)VirtualKey.Back,
            ConsoleKey.Delete => (int)VirtualKey.Delete,
            ConsoleKey.Insert => (int)VirtualKey.Insert,
            ConsoleKey.Home => (int)VirtualKey.Home,
            ConsoleKey.End => (int)VirtualKey.End,
            ConsoleKey.PageUp => (int)VirtualKey.PageUp,
            ConsoleKey.PageDown => (int)VirtualKey.PageDown,
            ConsoleKey.LeftArrow => (int)VirtualKey.LeftArrow,
            ConsoleKey.RightArrow => (int)VirtualKey.RightArrow,
            ConsoleKey.UpArrow => (int)VirtualKey.UpArrow,
            ConsoleKey.DownArrow => (int)VirtualKey.DownArrow,
            ConsoleKey.F1 => (int)VirtualKey.F1,
            ConsoleKey.F2 => (int)VirtualKey.F2,
            ConsoleKey.F3 => (int)VirtualKey.F3,
            ConsoleKey.F4 => (int)VirtualKey.F4,
            ConsoleKey.F5 => (int)VirtualKey.F5,
            ConsoleKey.F6 => (int)VirtualKey.F6,
            ConsoleKey.F7 => (int)VirtualKey.F7,
            ConsoleKey.F8 => (int)VirtualKey.F8,
            ConsoleKey.F9 => (int)VirtualKey.F9,
            ConsoleKey.F10 => (int)VirtualKey.F10,
            ConsoleKey.F11 => (int)VirtualKey.F11,
            ConsoleKey.F12 => (int)VirtualKey.F12,
            _ when key >= ConsoleKey.D0 && key <= ConsoleKey.D9 =>
                (int)(key - ConsoleKey.D0 + VirtualKey.D0),
            _ when key >= ConsoleKey.A && key <= ConsoleKey.Z =>
                (int)(key - ConsoleKey.A + VirtualKey.A),
            _ => 0
        };
    }
}