using System.Text.Json.Nodes;

namespace TreeTool;

/// <summary>
/// Turns config/looks.json into WLED presets. A look is one effect applied to every strip, with
/// cone-aware tweaks: a per-strip offset ("twist") that turns bands into spirals, a direction
/// flip for odd or even strips, and overrides for groups of strips (odd, even, N, E, S, W).
/// </summary>
public static class Looks
{
    /// <summary>The /json/state body that shows a look on one controller.</summary>
    public static JsonObject State(TreeLayout layout, Controller controller, JsonNode look,
        Func<string, int> effectId, Func<string, int> paletteId)
    {
        var segs = new JsonArray();
        foreach (var s in LayoutMath.Segments(layout, controller))
        {
            var p = Merge(look, s.Strip, layout.StripCount);
            var twist = p["twist"]?.GetValue<int>() ?? 0;
            var length = s.Stop - s.Start;
            var flip = Flip(p["flip"]?.ToString(), s.Strip);
            segs.Add(new JsonObject
            {
                ["id"] = s.SegmentId,
                ["start"] = s.Start,
                ["stop"] = s.Stop,
                ["n"] = $"Strip {s.Strip}",
                ["on"] = true,
                ["fx"] = effectId(p["fx"]?.ToString() ?? "Solid"),
                ["pal"] = paletteId(p["pal"]?.ToString() ?? "Default"),
                ["sx"] = p["sx"]?.GetValue<int>() ?? 128,
                ["ix"] = p["ix"]?.GetValue<int>() ?? 128,
                ["col"] = Colours(p["colours"]),
                ["of"] = ((s.Strip - 1) * twist % length + length) % length,
                ["rev"] = s.RunsDown ^ flip,
            });
        }
        return new JsonObject { ["on"] = true, ["seg"] = segs };
    }

    /// <summary>The look's own settings, overlaid by every group the strip belongs to.</summary>
    static JsonObject Merge(JsonNode look, int strip, int stripCount)
    {
        var merged = new JsonObject();
        foreach (var (key, value) in look.AsObject())
            if (key != "groups") merged[key] = value?.DeepClone();
        if (look["groups"] is JsonObject groups)
            foreach (var (group, overrides) in groups)
                if (InGroup(group, strip, stripCount) && overrides is JsonObject o)
                    foreach (var (key, value) in o)
                        merged[key] = value?.DeepClone();
        return merged;
    }

    /// <summary>Groups match the phone app: odd/even, a compass quarter, or a strip number.</summary>
    public static bool InGroup(string group, int strip, int stripCount)
    {
        var degrees = (strip - 1) * 360.0 / stripCount;
        bool Quarter(double centre)
        {
            var d = Math.Abs((degrees - centre + 540) % 360 - 180);
            return d < 45 || (d == 45 && centre != 270);
        }
        return group switch
        {
            "odd" => strip % 2 == 1,
            "even" => strip % 2 == 0,
            "N" => Quarter(0),
            "E" => Quarter(90),
            "S" => Quarter(180),
            "W" => Quarter(270),
            _ => int.TryParse(group, out var n) && n == strip,
        };
    }

    static bool Flip(string? flip, int strip) => flip switch
    {
        "odd" => strip % 2 == 1,
        "even" => strip % 2 == 0,
        "all" => true,
        _ => false,
    };

    static JsonArray Colours(JsonNode? colours)
    {
        var result = new JsonArray();
        var list = colours?.AsArray().Select(c => c?.ToString() ?? "#000000").ToList() ?? [];
        while (list.Count < 3) list.Add("#000000");
        foreach (var hex in list.Take(3))
        {
            var v = Convert.ToInt32(hex.TrimStart('#'), 16);
            result.Add(new JsonArray(v >> 16, (v >> 8) & 255, v & 255));
        }
        return result;
    }

    /// <summary>A saved playlist preset that cycles through looks.</summary>
    public static JsonObject ShowPreset(JsonNode show)
    {
        var ids = show["looks"]!.AsArray().Select(n => n!.GetValue<int>()).ToList();
        var tenths = (int)Math.Round((show["seconds"]?.GetValue<double>() ?? 60) * 10);
        var fade = (int)Math.Round((show["fadeSeconds"]?.GetValue<double>() ?? 2) * 10);
        return new JsonObject
        {
            ["psave"] = show["id"]!.GetValue<int>(),
            ["n"] = show["name"]!.ToString(),
            ["o"] = true, // store the playlist itself, not the current state
            ["on"] = true,
            ["playlist"] = new JsonObject
            {
                ["ps"] = new JsonArray(ids.Select(i => (JsonNode)i).ToArray()),
                ["dur"] = new JsonArray(ids.Select(_ => (JsonNode)tenths).ToArray()),
                ["transition"] = new JsonArray(ids.Select(_ => (JsonNode)fade).ToArray()),
                ["repeat"] = 0,
                ["end"] = 0,
            },
        };
    }
}
