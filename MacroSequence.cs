using System.Collections;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace DevelopingInsanity.KeyMacro.Macros;

public partial class MacroSequence:IEnumerable<MacroItem>
{
    public List<MacroItem> Items { get; private set; }

    internal MacroSequence() : this(null)
    {
    }

    public MacroSequence(string? targetWindow)
    {
        Items = [];
        TargetWindow = targetWindow;
    }

    public void AddItem(MacroItem item)
    {
        Items.Add(item);
    }

    public void RemoveItem(MacroItem item)
    {
        Items.Remove(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public string? TargetWindow { get; private set; }

    public int Count => Items.Count;

    public MacroItem this[int index] => Items[index];

    private static string? FindTargetWindow(string filePath)
    {
        var source = File.ReadAllLines(filePath);

        foreach(var line in source)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            if (parts[0].Equals("Target", StringComparison.OrdinalIgnoreCase))
            {
                Regex r = TargetWindowRegex();
                string subject = line[(parts[0].Length + 1)..];
                if(r.IsMatch(subject))
                {
                    var match = r.Match(subject);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
        }

        return null;
    }

    public static MacroSequence FromFile(string filePath)
    { 
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Macro file not found.", filePath);
        }

        MacroSequence sequence = new()
        {
            TargetWindow = FindTargetWindow(filePath)
        };

        if (sequence.TargetWindow == null)
        {
            throw new InvalidOperationException("No target window specified in the macro file.");
        }

        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if(line.Trim().StartsWith('#')) continue; // comment lines

            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            if (parts[0].Equals("Target", StringComparison.OrdinalIgnoreCase))
                continue;

            var action = Enum.Parse<MacroAction>(parts[0], true);
            var parameters = parts[1..];

            MacroItem item = action switch
            {
                MacroAction.SendKey => new SendKeyMacro(sequence.TargetWindow, parameters[0]),
                MacroAction.PressKey => new PressKeyMacro(sequence.TargetWindow, parameters[0]),
                MacroAction.ReleaseKey => new ReleaseKeyMacro(sequence.TargetWindow, parameters[0]),
                MacroAction.Wait => new WaitMacro(int.Parse(parameters[0])),
                MacroAction.PressButton => parameters.Length switch
                {
                    3 when Enum.TryParse<MouseButton>(parameters[0], true, out var button) && int.TryParse(parameters[1], out var x) && int.TryParse(parameters[2], out var y) =>
                        new PressButtonMacro(sequence.TargetWindow, button, new System.Drawing.Point(x, y)),
                    _ => throw new ArgumentException("Invalid parameters for PressButton action.")
                },
                MacroAction.ReleaseButton => parameters.Length switch
                {
                    3 when Enum.TryParse<MouseButton>(parameters[0], true, out var button) && int.TryParse(parameters[1], out var x) && int.TryParse(parameters[2], out var y) =>
                        new ReleaseButtonMacro(sequence.TargetWindow, button, new System.Drawing.Point(x, y)),
                    _ => throw new ArgumentException("Invalid parameters for ReleaseButton action.")
                },
                _ => throw new NotSupportedException($"Unsupported macro action: {action}")
            };

            sequence.AddItem(item);
        }

        return sequence;
    }

    public void SaveToFile(string filePath)
    {
        if(string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Enter a file path", nameof(filePath));
        }

        if (TargetWindow == null)
        {
            throw new InvalidOperationException("Macro sequence cannot be saved without TargetWindow");
        }

        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using StreamWriter writer = new(File.Create(filePath));
        writer.Write(ToString());
    }

    public IEnumerator<MacroItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Target \"{TargetWindow??string.Empty}\"");
        foreach(var item in Items)
        {
            sb.AppendLine(item.ToString());
        }
        return sb.ToString();
    }

    [GeneratedRegex("\"([^\"]+)\"")]
    private static partial Regex TargetWindowRegex();
}