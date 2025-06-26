using DevelopingInsanity.KeyMacro;
using DevelopingInsanity.KeyMacro.Macros;

var parameters = InputParameters.Parse(args);

if (parameters.Record)
{
    //recording mode
    Console.WriteLine("Recording mode: pick a key to use to stop recording.");
    ConsoleKeyInfo stopKey = Console.ReadKey();

    Console.WriteLine($"\nYour chosen stop key is '{stopKey.Key}'\n");

    string? targetWindow = null;

    WindowEnumerator winEnum = WindowEnumerator.Create();

    if(winEnum.Count == 0)
    {
        Console.WriteLine("No open windows to target found. Exiting.");
        return;
    }

    Console.WriteLine("Currently open windows:");
    int i = 0;
    foreach (var title in winEnum)
    {
        Console.WriteLine($"\t[{(++i).ToString().PadLeft(3, '0')}]{title}");
    }

    while (targetWindow == null)
    {
        Console.Write($"Pick a macro target window [1-{winEnum.Count}]: ");
        string input = Console.ReadLine()?.Trim() ?? string.Empty;
        if (int.TryParse(input, out int index) && index >= 1 && index <= winEnum.Count)
        {
            targetWindow = winEnum[index - 1];
        }
        else
        {
            continue;
        }
    }

    var sequence = new MacroSequence(targetWindow);

    var recorder = MacroRecorder.Install();
    recorder.RecordAdded += Recorder_RecordAdded;

    Console.WriteLine($"Recording started, target window set to \"{targetWindow}\".\n Press '{stopKey.Key}' anytime to stop recording.");
   
    //two inelegant lines of code that I'm too comfortable keeping here to move elsewhere
    Thread.Sleep(2000);
    Win32Native.SetForegroundWindow(Win32Native.FindWindow(null, targetWindow));

    recorder.LogKeys(stopKey.Key);
    recorder.AppendSequence(sequence);
    sequence.SaveToFile(parameters.Path);
}
else
{
    //execution mode

    if (!File.Exists(parameters.Path))
    {
        Console.WriteLine($"Error: Macro file '{parameters.Path}' not found.");
        return;
    }

    var sequence = MacroSequence.FromFile(parameters.Path);
    Console.WriteLine($"Loaded macro sequence with {sequence.Count} items.");

    if (parameters.Delay > 0)
    {
        Console.WriteLine($"Starting macro sequence in {parameters.Delay} seconds...");
        Thread.Sleep(parameters.Delay * 1000);
    }

    //two inelegant lines of code that I'm too comfortable keeping here to move elsewhere
    Win32Native.SetForegroundWindow(Win32Native.FindWindow(null, sequence.TargetWindow));
    Thread.Sleep(2000);

    int iteration = 0;

    do
    {
        Console.WriteLine($"Iteration #{++iteration}");
        int i = 0;

        foreach (var item in sequence)
        {
            Console.WriteLine($"\t{++i}/{sequence.Count}: {item}");
            if (!item.Execute())
            {
                Console.WriteLine($"\tExecution break({item.LastError}): {item}");
                break;
            }

            if (!parameters.Silent)
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Console.Beep(1320, 50);
                    Thread.Sleep(10);
                    Console.Beep(1580, 50);
                });
                
        }

        Thread.Sleep(1000);
    }
    while (iteration < parameters.Loops);
}

static void Recorder_RecordAdded(object? sender, RecordAddedEventArgs e)
{
    Console.WriteLine($"\tRecord Added: {e.Record}");
}