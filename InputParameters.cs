namespace DevelopingInsanity.KeyMacro;


public class InputParameters(string path, int delay = 0, int numberOfLoops = 0, bool recording = false, bool silent = false)
{
    public string Path { get; set; } = path;
    public int Delay { get; set; } = delay;
    public int Loops { get; set; } = numberOfLoops;
    public bool LoopExecution => Loops > 0;
    public bool Record { get; set; } = recording;
    public bool Silent { get; set; } = silent;

    //allowed parameters:
    // -f, --file: Path to the macro file
    // -d, --delay: Delay in milliseconds before executing the macro
    // -s, --silent: If specified, macro execution won't play sounds
    // -l, --loop: if specified, loop for a specified number of iterations
    // --record: Record a new macro sequence interactively
    public static InputParameters Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage:\n\tKeyMacro -f <path> [-d <delay_seconds>][-l <loops>][-s]\n\tKeyMacro --record -f <path>\n");
        }

        string path = string.Empty;
        int delay = 0, loops = 0;
        bool record = false, silent = false;

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
                    if (i + 1 < args.Length)
                    {
                        loops = int.Parse(args[++i]);
                    }
                    else
                    {
                        throw new ArgumentException("Path argument requires a value.");
                    }
                    break;
                case "--record":
                    {
                        record = true;
                    }
                    break;
                case "-s":
                case "--silent":
                    {
                        silent = true;
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path is required.");
        }

        return new InputParameters(path, delay, loops, record, silent);
    }
}