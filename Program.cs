using DevelopingInsanity.KeyMacro;
using DevelopingInsanity.KeyMacro.Macros;

var parameters = InputParameters.Parse(args);

if (!File.Exists(parameters.Path))
{
    Console.WriteLine($"Error: Macro file '{parameters.Path}' not found.");
    return;
}

var sequence = MacroSequence.FromFile(parameters.TargetWindow, parameters.Path);
Console.WriteLine($"Loaded macro sequence with {sequence.Count} items.");

if (parameters.Delay > 0)
{
    Console.WriteLine($"Starting macro sequence in {parameters.Delay} seconds...");
    Thread.Sleep(parameters.Delay * 1000);
}

int iteration = 0, i = 0;

do
{
    Console.WriteLine($"Iteration {++iteration}");
    i = 0;

    foreach (var item in sequence)
    {
        Console.WriteLine($"{++i}/{sequence.Count}: {item}");
        if (!item.Execute())
        {
            Console.WriteLine($"Execution break({item.LastError}): {item}");
            break;
        }
        Thread.Sleep(50);
    }

    Thread.Sleep(1000);
}
while (parameters.Loop);


