namespace DevelopingInsanity.KeyMacro;


public class InputParameters(string path, string targetWindow, int delay = 0, bool loop = false)
{
    public string Path { get; set; } = path;
    public int Delay { get; set; } = delay;
    public string TargetWindow { get; set; } = targetWindow;
    public bool Loop { get; set; } = loop;

    //allowed parameters:
    // -f, --file: Path to the macro file
    // -d, --delay: Delay in milliseconds before executing the macro
    // -w, --window: Target window for the macro
    // -l, --loop: if specified, loop forever
    public static InputParameters Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: KeyMacro -f <path> -w <window_name> [-d <delay_seconds>][-l]");
        }

        string path = string.Empty, window = string.Empty;
        int delay = 0;
        bool loop = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-f":
                case "--file":
                    if (i + 1 < args.Length)
                    {
                        path = args[++i];
                    }
                    else
                    {
                        throw new ArgumentException("Path argument requires a value.");
                    }
                    break;
                case "-w":
                case "--window":
                    if (i + 1 < args.Length)
                    {
                        window = args[++i];
                    }
                    else
                    {
                        throw new ArgumentException("Path argument requires a value.");
                    }
                    break;
                case "-d":
                case "--delay":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out delay) && delay >= 0)
                    {
                        // Valid delay
                    }
                    else
                    {
                        throw new ArgumentException("Delay argument requires a valid integer value.");
                    }
                    break;
                case "-l":
                case "--loop":
                    loop = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path is required.");
        }

        if (string.IsNullOrEmpty(window))
        {
            throw new ArgumentException("Path is required.");
        }

        return new InputParameters(path, window, delay, loop);
    }
}