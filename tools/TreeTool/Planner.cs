using System.Text.Json.Nodes;

namespace TreeTool;

/// <summary>Builds the WLED config/state that a layout requires, and prints how it differs from the device.</summary>
public static class Planner
{
    public static JsonObject DesiredCfg(TreeLayout layout, Controller controller)
    {
        var buses = LayoutMath.Buses(layout, controller);
        var ins = new JsonArray();
        foreach (var bus in buses)
        {
            ins.Add(new JsonObject
            {
                ["start"] = bus.Start,
                ["len"] = bus.Length,
                ["pin"] = new JsonArray(bus.Gpio),
                ["order"] = layout.ColorOrder,
                ["rev"] = false,
                ["skip"] = 0,
                ["type"] = layout.LedType,
                ["ref"] = false,
                ["rgbwm"] = 0,
                ["freq"] = 0,
            });
        }
        var cfg = new JsonObject
        {
            ["hw"] = new JsonObject
            {
                ["led"] = new JsonObject
                {
                    ["total"] = buses.Sum(b => b.Length),
                    ["maxpwr"] = layout.Abl.MaxMa,
                    ["ledma"] = layout.Abl.LedMa,
                    ["ins"] = ins,
                },
            },
            ["def"] = new JsonObject { ["ps"] = layout.LayoutPreset },
        };
        // Only the AP behaviour is sent; its name and password stay as they are on the board.
        if (DesiredApBehaviour(layout) is int behav)
            cfg["ap"] = new JsonObject { ["behav"] = behav };
        return cfg;
    }

    /// <summary>WLED ap.behav: 0 = no connection after boot, 1 = disconnected, 2 = always, 3 = never. Null = leave it.</summary>
    static int? DesiredApBehaviour(TreeLayout layout) => layout.Network is { ApAlways: true } ? 2 : null;

    /// <summary>True when the device's LED outputs, limiter and boot preset already match the layout.</summary>
    public static bool CfgMatches(TreeLayout layout, Controller controller, JsonNode currentCfg)
    {
        var desired = DesiredCfg(layout, controller);
        var want = desired["hw"]!["led"]!;
        var have = currentCfg["hw"]?["led"];
        if (have is null) return false;
        foreach (var key in new[] { "total", "maxpwr", "ledma" })
            if (have[key]?.ToString() != want[key]!.ToString()) return false;
        if (currentCfg["def"]?["ps"]?.ToString() != desired["def"]!["ps"]!.ToString()) return false;
        if (DesiredApBehaviour(layout) is int behav && currentCfg["ap"]?["behav"]?.ToString() != behav.ToString()) return false;

        var haveIns = have["ins"]?.AsArray() ?? [];
        var wantIns = want["ins"]!.AsArray();
        if (haveIns.Count != wantIns.Count) return false;
        for (var i = 0; i < wantIns.Count; i++)
            foreach (var key in new[] { "start", "len", "type", "order" })
                if (haveIns[i]?[key]?.ToString() != wantIns[i]![key]!.ToString()) return false;
        for (var i = 0; i < wantIns.Count; i++)
            if (haveIns[i]?["pin"]?[0]?.ToString() != wantIns[i]!["pin"]![0]!.ToString()) return false;
        return true;
    }

    /// <summary>Segment definitions, plus deletions (stop = 0) for any leftover segment ids.</summary>
    public static JsonObject DesiredSegmentsState(TreeLayout layout, Controller controller, int maxSegments)
    {
        var segs = new JsonArray();
        var segments = LayoutMath.Segments(layout, controller);
        foreach (var s in segments)
        {
            segs.Add(new JsonObject
            {
                ["id"] = s.SegmentId,
                ["start"] = s.Start,
                ["stop"] = s.Stop,
                ["rev"] = s.RunsDown,
                ["n"] = $"Strip {s.Strip}",
                ["on"] = true,
                ["fx"] = 0,
                ["grp"] = 1,
                ["spc"] = 0,
                ["bri"] = 255,
                ["frz"] = false,
                ["col"] = new JsonArray(new JsonArray(255, 160, 60), new JsonArray(0, 0, 0), new JsonArray(0, 0, 0)),
            });
        }
        for (var id = segments.Count; id < maxSegments; id++)
            segs.Add(new JsonObject { ["id"] = id, ["stop"] = 0 });

        return new JsonObject { ["on"] = true, ["bri"] = 80, ["seg"] = segs };
    }

    public static void PrintPlan(TreeLayout layout, Controller controller, JsonNode currentCfg, JsonNode currentState)
    {
        var led = currentCfg["hw"]?["led"];
        Console.WriteLine($"== {controller.Name} ({controller.Host}) ==");
        Console.WriteLine();
        Console.WriteLine("LED outputs            current                      desired");

        var currentIns = led?["ins"]?.AsArray() ?? [];
        var buses = LayoutMath.Buses(layout, controller);
        var rows = Math.Max(currentIns.Count, buses.Count);
        for (var i = 0; i < rows; i++)
        {
            var cur = i < currentIns.Count
                ? $"GPIO {currentIns[i]?["pin"]?[0],-2} start {currentIns[i]?["start"],-5} len {currentIns[i]?["len"],-5}"
                : "(none)";
            var want = i < buses.Count
                ? $"GPIO {buses[i].Gpio,-2} start {buses[i].Start,-5} len {buses[i].Length,-5}"
                : "(remove)";
            Console.WriteLine($"  output {i + 1}  {cur,-30} {want}{Marker(cur, want)}");
        }
        Row("total LEDs", led?["total"], buses.Sum(b => b.Length));
        Row("type", currentIns.Count > 0 ? currentIns[0]?["type"] : null, layout.LedType);
        Row("colour order", currentIns.Count > 0 ? currentIns[0]?["order"] : null, layout.ColorOrder);
        Row("limiter mA", led?["maxpwr"], layout.Abl.MaxMa);
        Row("mA per LED", led?["ledma"], layout.Abl.LedMa);
        Row("boot preset", currentCfg["def"]?["ps"], layout.LayoutPreset);
        if (DesiredApBehaviour(layout) is int behav)
            Row("WLED-AP (2 = always)", currentCfg["ap"]?["behav"], behav);

        Console.WriteLine();
        Console.WriteLine("Segments (one per strip; 'down' = reversed so LED 0 is at the bottom)");
        var currentSegs = currentState["seg"]?.AsArray() ?? [];
        Console.WriteLine($"  current: {currentSegs.Count} segment(s): " +
            string.Join(", ", currentSegs.Select(s => $"{s?["start"]}-{s?["stop"]}")));
        foreach (var s in LayoutMath.Segments(layout, controller))
            Console.WriteLine($"  seg {s.SegmentId,2}  Strip {s.Strip,2}  LEDs {s.Start,4}-{s.Stop - 1,-4}  {(s.RunsDown ? "down (rev)" : "up")}");
        Console.WriteLine();
    }

    static void Row(string label, JsonNode? current, object desired)
    {
        var cur = current?.ToString() ?? "?";
        var want = desired.ToString() ?? "";
        Console.WriteLine($"  {label,-20} {cur,-30} {want}{Marker(cur, want)}");
    }

    static string Marker(string current, string desired) => current == desired ? "" : "   <- change";
}
