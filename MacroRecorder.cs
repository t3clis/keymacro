using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DevelopingInsanity.KeyMacro.Macros;

public record class MacroRecord(VirtualKey Key, KeyOp Operation, long TimeOffset)
{
    public override string ToString()
    {
        return $"[{TimeOffset.ToString().PadLeft(19, '0')}] {Operation} {Key}";
    }
}

public class RecordAddedEventArgs : EventArgs
{
    public MacroRecord Record { get; }
    public RecordAddedEventArgs(MacroRecord record)
    {
        Record = record;
    }
}

public class MacroRecorder : IDisposable
{
    private nint _hookHandle = nint.Zero;
    private bool _started = false;
    private bool disposedValue;
    private readonly List<MacroRecord> _records = [];
    private int _vkStop = 0;
    private long? _lastTimestamp = null;
    private bool _firstEnterUpIgnored = false;
    private Win32Native.LowLevelKeyboardProc _keyboardProc;

    public event EventHandler<RecordAddedEventArgs>? RecordAdded;

    public int RecordedCount
    {
        get
        {
            lock (_records)
            {
                return _records.Count;
            }
        }
    }

    public bool IsStarted
    {
        get
        {
            lock (this)
            {
                return _started;
            }
        }
    }

    public ConsoleKey? StopKey
    {
        get
        {
            return Win32Native.ToConsoleKey(_vkStop);
        }

        private set
        {
            _vkStop = Win32Native.ToVirtualKeyCode(value);
        }
    }

    internal MacroRecorder()
    {
        _keyboardProc = HookCallback;
    }

    private nint HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {

        if (nCode >= 0)
        {
            if (!IsStarted)
                return Win32Native.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            int evCode = wParam.ToInt32();
            int vkCode = Marshal.ReadInt32(lParam);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), timeoffset;

            KeyOp op = KeyOp.None;
            VirtualKey vk = (VirtualKey)vkCode;
            ConsoleKey? ck = Win32Native.ToConsoleKey(vkCode);
            timeoffset = _lastTimestamp.HasValue ? timestamp - _lastTimestamp.Value : 0;
            _lastTimestamp = timestamp;

            if (evCode == Win32Native.WM_KEYDOWN || evCode == Win32Native.WM_SYSKEYDOWN)
            {
                op = KeyOp.Down;

                if (ck != null && ck == StopKey)
                {
                    lock (this)
                    {
                        _started = false;
                    }

                    return Win32Native.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
                }
            }
            else if (evCode == Win32Native.WM_KEYUP || evCode == Win32Native.WM_SYSKEYUP)
            {
                op = KeyOp.Up;
            }

            if (op == KeyOp.Up && vk == VirtualKey.Enter && !_firstEnterUpIgnored)
            {
                //ignore first Enter Up event after starting recording, it's spurious
                _firstEnterUpIgnored = true;
                return Win32Native.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }
            else if (!_firstEnterUpIgnored)
            {
                //if we don't get any first enter up something is up, but we can't ignore next enter up anyway, so we won't
                _firstEnterUpIgnored = true;
            }

            lock (_records)
            {
                var record = new MacroRecord(vk, op, timeoffset);
                _records.Add(record);
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    RecordAdded?.Invoke(this, new RecordAddedEventArgs(record));
                });
            }
        }

        return Win32Native.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public static MacroRecorder Install()
    {
        MacroRecorder recorder = new();

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule ?? throw new InvalidOperationException("Current process has no main module.");
        recorder._hookHandle = Win32Native.SetWindowsHookEx(Win32Native.WH_KEYBOARD_LL, recorder._keyboardProc, Win32Native.GetModuleHandle(curModule.ModuleName), 0);

        if (recorder._hookHandle == nint.Zero)
        {
            throw new InvalidOperationException($"Failed to install hook ({Marshal.GetLastWin32Error()})");
        }

        return recorder;
    }

    public void LogKeys(ConsoleKey stopKey)
    {
        LogKeys(stopKey, false);
    }

    public void LogKeys(ConsoleKey stopKey, bool append)
    {
        StopKey = stopKey;

        if (StopKey == null)
        {
            throw new ArgumentException("Enter an acceptable stop key.", nameof(stopKey));
        }

        lock (this)
        {
            if (_started)
            {
                throw new InvalidOperationException("MacroRecorder is already started.");
            }

            if (!append)
            {
                _lastTimestamp = null;
                _records.Clear(); //no need to lock records unless code changes, since it shouldn't change while not started.
            }

            _firstEnterUpIgnored = false;
            _started = true;
        }

        bool started = IsStarted;

        while (started)
        {
            MSG msg = new();


            if (Win32Native.PeekMessage(out msg, nint.Zero, 0, 0, 1))
            {
                Win32Native.TranslateMessage(ref msg);
                Win32Native.DispatchMessage(ref msg);
            }
            else
            {
                Thread.Sleep(10); //reduce CPU usage while waiting for messages
            }

            started = IsStarted;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _records.Clear();
            }

            Win32Native.UnhookWindowsHookEx(_hookHandle);
            disposedValue = true;
        }
    }

    ~MacroRecorder()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal void AppendSequence(MacroSequence sequence)
    {
        lock (this)
        {
            if (_started)
                throw new InvalidOperationException("Cannot append sequence while recording is in progress.");
        }

        if (sequence.TargetWindow == null)
        {
            throw new ArgumentException("Cannot work on a sequence not targetting any window.");
        }

        foreach (var entry in _records)
        {
            if(entry.TimeOffset > 0)
                sequence.AddItem(new WaitMacro((int)entry.TimeOffset));

            switch(entry.Operation)
            {
                case KeyOp.Down:
                    sequence.AddItem(new PressKeyMacro(sequence.TargetWindow, entry.Key.ToString()));
                    break;
                case KeyOp.Up:
                    sequence.AddItem(new ReleaseKeyMacro(sequence.TargetWindow, entry.Key.ToString()));
                    break;
                default:
                    //ignore None operation
                    break;
            }
        }
    }
}
