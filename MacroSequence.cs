using System.Collections;
using System.Data.Common;

namespace DevelopingInsanity.KeyMacro.Macros;

public class MacroSequence:IEnumerable<MacroItem>
{
    public List<MacroItem> Items { get; private set; }

    public MacroSequence()
    {
        Items = [];
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

    public int Count => Items.Count;

    public MacroItem this[int index] => Items[index];

    public static MacroSequence FromFile(string windowName, string filePath)
    { 
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Macro file not found.", filePath);
        }

        var sequence = new MacroSequence();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if(line.Trim().StartsWith("#")) continue; // Skip comments

            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var action = Enum.Parse<MacroAction>(parts[0], true);
            var parameters = parts[1..];

            MacroItem item = action switch
            {
                MacroAction.SendKey => new SendKeyMacro(windowName, parameters[0]),
                MacroAction.PressKey => new PressKeyMacro(windowName, parameters[0]),
                MacroAction.ReleaseKey => new ReleaseKeyMacro(windowName, parameters[0]),
                MacroAction.Wait => new WaitMacro(int.Parse(parameters[0])),
                _ => throw new NotSupportedException($"Unsupported macro action: {action}")
            };

            sequence.AddItem(item);
        }

        return sequence;
    }

    public IEnumerator<MacroItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}