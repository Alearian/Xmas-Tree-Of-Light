using System.Text.Json;

namespace TreeTool;

public sealed record Abl(int LedMa, int MaxMa);

public sealed record Output(int Gpio, int[] Strips);

public sealed record Controller(string Name, string Host, Output[] Outputs);

/// <summary>ApAlways keeps WLED-AP up alongside the home Wi-Fi (WLED ap.behav 2), so either network reaches the board.</summary>
public sealed record Network(bool ApAlways, string? ApAddress);

public sealed record TreeLayout(
    string Name,
    int LedsPerStrip,
    int StripCount,
    double HeightMetres,
    int LedType,
    int ColorOrder,
    Abl Abl,
    int LayoutPreset,
    Controller[] Controllers,
    int[]? Attached = null,
    Network? Network = null)
{
    /// <summary>Strips physically wired up. The rest are still rendered by WLED (and previewed by the app).</summary>
    public IReadOnlyCollection<int> AttachedStrips =>
        Attached ?? Controllers.SelectMany(c => c.Outputs).SelectMany(o => o.Strips).ToArray();

    static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static TreeLayout Load(string path)
    {
        var layout = JsonSerializer.Deserialize<TreeLayout>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException($"Empty layout: {path}");
        layout.Validate();
        return layout;
    }

    void Validate()
    {
        var all = Controllers.SelectMany(c => c.Outputs).SelectMany(o => o.Strips).ToList();
        if (all.Count != StripCount)
            throw new InvalidDataException($"stripCount is {StripCount} but outputs list {all.Count} strips");
        if (all.Distinct().Count() != all.Count)
            throw new InvalidDataException("A strip number appears on more than one output");
        if (all.Any(s => s < 1 || s > StripCount))
            throw new InvalidDataException($"Strip numbers must be 1..{StripCount}");
        if (Attached?.Any(s => !all.Contains(s)) == true)
            throw new InvalidDataException("attached lists a strip that isn't on any output");
        foreach (var c in Controllers)
            if (c.Outputs.Length > 4)
                throw new InvalidDataException($"{c.Name}: a Dig-Quad has at most 4 outputs");
    }
}

/// <summary>One physical strip as WLED sees it on a given controller.</summary>
public sealed record StripSegment(int Strip, int SegmentId, int Start, int Stop, bool RunsDown);

/// <summary>One WLED LED output (bus) as it should appear in /json/cfg hw.led.ins.</summary>
public sealed record Bus(int Gpio, int Start, int Length);

public static class LayoutMath
{
    /// <summary>Buses for a controller: each output's strips laid end to end.</summary>
    public static List<Bus> Buses(TreeLayout layout, Controller controller)
    {
        var buses = new List<Bus>();
        var start = 0;
        foreach (var output in controller.Outputs)
        {
            var length = output.Strips.Length * layout.LedsPerStrip;
            buses.Add(new Bus(output.Gpio, start, length));
            start += length;
        }
        return buses;
    }

    /// <summary>
    /// One segment per strip. Chains zig-zag (1st strip up, 2nd down, ...), so strips that
    /// run down are reversed; LED 0 of every segment is then at the bottom of the tree.
    /// </summary>
    public static List<StripSegment> Segments(TreeLayout layout, Controller controller)
    {
        var segments = new List<StripSegment>();
        var start = 0;
        foreach (var output in controller.Outputs)
        {
            for (var i = 0; i < output.Strips.Length; i++)
            {
                segments.Add(new StripSegment(output.Strips[i], segments.Count, start, start + layout.LedsPerStrip, i % 2 == 1));
                start += layout.LedsPerStrip;
            }
        }
        return segments;
    }
}
