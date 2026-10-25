using System.Text.Json.Nodes;

namespace TreeTool;

/// <summary>
/// Turns config/looks.json into WLED presets. A look is one effect applied to every strip, with
/// cone-aware tweaks: a per-strip offset ("twist") that turns bands into spirals, a direction
/// flip for odd or even strips, and overrides for groups of strips (odd, even, N, E, S, W).
/// </summary>
public static class Looks
{
    /// <summary>Preset that turns the lights off. Shows end on it, and the app's Stop and daily end times use it.</summary>
    public const int LightsOff = 250;

    /// <summary>Segments WLED 0.14 allows on an ESP32 (/json/info leds.maxseg).</summary>
    public const int MaxSegments = 32;

    public static JsonObject LightsOffPreset() => new()
    {
        ["psave"] = LightsOff,
        ["n"] = "Lights off",
        ["o"] = true,
        ["on"] = false,
    };

    /// <summary>
    /// Hidden playlist step between the looks of a show: every segment fades to brightness 0 (WLED then
    /// turns the segment off), and the next look turns its segments back on at 255, fading up. So every
    /// look sets "on" and "bri" on each of its segments. WLED 0.14 swaps effects instantly, so this dip
    /// is what makes a visible fade between two different effects.
    /// </summary>
    public const int FadeDown = 249;

    public static JsonObject FadeDownPreset()
    {
        var segs = new JsonArray();
        for (var id = 0; id < MaxSegments; id++)   // ids that don't exist are ignored
            segs.Add(new JsonObject { ["id"] = id, ["bri"] = 0 });
        return new JsonObject { ["psave"] = FadeDown, ["n"] = "Fade down", ["o"] = true, ["seg"] = segs };
    }

    /// <summary>
    /// The /json/state body that shows a look on one controller. An optional "overlay" adds a second
    /// segment over each strip, drawn on top of the first with the effect's Overlay option (o2), for
    /// example white Drip flakes over coloured Meteor snow. Looks without one delete those segments.
    /// </summary>
    public static JsonObject State(TreeLayout layout, Controller controller, JsonNode look,
        Func<string, int> effectId, Func<string, int> paletteId)
    {
        if (look["across"] is JsonObject across)
            return AcrossState(layout, controller, look, across, effectId, paletteId);
        var segs = new JsonArray();
        var strips = LayoutMath.Segments(layout, controller);
        foreach (var s in strips)
            segs.Add(Segment(s, s.SegmentId, $"Strip {s.Strip}", Merge(look, s.Strip, layout.StripCount),
                effectId, paletteId));
        foreach (var s in strips)
        {
            var id = s.SegmentId + strips.Count;
            if (look["overlay"] is JsonObject overlay)
            {
                var seg = Segment(s, id, $"Strip {s.Strip} overlay", Merge(overlay, s.Strip, layout.StripCount),
                    effectId, paletteId);
                seg["o2"] = true;
                // the ids may have been Laser Search bands
                seg["grp"] = 1; seg["spc"] = 0; seg["bri"] = 255;
                segs.Add(seg);
            }
            else
                segs.Add(new JsonObject { ["id"] = id, ["stop"] = 0 });
        }
        // an "across" look can use every id up to the limit
        for (var id = 2 * strips.Count; id < MaxSegments; id++)
            segs.Add(new JsonObject { ["id"] = id, ["stop"] = 0 });
        return new JsonObject { ["on"] = true, ["seg"] = segs };
    }

    /// <summary>
    /// A look that runs one effect across the strips instead of along them, for example Scanner sweeping a
    /// lit strip round the tree ("Laser Search"). Grouping makes every strip one pixel of a segment, and spacing
    /// skips the rest of the strip, so one segment covers one height band of every strip. The strip's height
    /// is split into bands, one segment each, with the first colour dimmer towards the top and bottom. The fade is symmetric, so it
    /// doesn't matter which way a strip runs. The strip segments stay (the app relies on them) but are frozen
    /// black, so they don't draw over the bands.
    /// </summary>
    static JsonObject AcrossState(TreeLayout layout, Controller controller, JsonNode look, JsonObject across,
        Func<string, int> effectId, Func<string, int> paletteId)
    {
        var strips = LayoutMath.Segments(layout, controller);
        var from = across["from"]?.GetValue<int>() ?? 1;
        var to = across["to"]?.GetValue<int>() ?? layout.StripCount;
        // the bands use the segment ids after the strips, and an odd number gives one middle band
        var bands = Math.Clamp(across["bands"]?.GetValue<int>() ?? 7, 1,
            Math.Min(MaxSegments - strips.Count, layout.LedsPerStrip));
        if (bands % 2 == 0) bands--;
        var endPercent = Math.Clamp(across["endBrightness"]?.GetValue<int>() ?? 100, 0, 100);
        var swept = strips.Where(s => s.Strip >= from && s.Strip <= to).OrderBy(s => s.Start).ToList();
        var length = layout.LedsPerStrip;
        if (swept.Count > 0 && swept[^1].Start - swept[0].Start != (swept.Count - 1) * length)
            throw new InvalidDataException($"{look["name"]}: strips {from}-{to} aren't next to each other on {controller.Name}");

        // Kept as small as possible: with 19 bands the full body restarted the board while saving the preset.
        var segs = new JsonArray();
        foreach (var s in strips)
            segs.Add(new JsonObject
            {
                ["id"] = s.SegmentId, ["fx"] = 0, ["col"] = new JsonArray(new JsonArray(0, 0, 0)),
                ["frz"] = swept.Count > 0, ["on"] = true, ["bri"] = 255,
            });

        // "half" pairs of end bands (bottom and top of the strip, same brightness), then the middle band
        var half = bands / 2;
        var width = length / bands;
        var p = Merge(look, from, layout.StripCount);
        var id = strips.Count;
        void Band(int offset, int size, int percent)
        {
            var first = swept[0].Start + offset;
            // Dim the band through its colour, not the segment's opacity: WLED applies opacity every time a
            // pixel is written, and fading effects (Scanner) rewrite their pixels each frame, so a dimmed band
            // would lose its trail almost at once.
            var col = Colours(p["colours"]);
            var eye = col[0]!.AsArray();
            for (var c = 0; c < eye.Count; c++)
                eye[c] = (int)Math.Round(eye[c]!.GetValue<int>() * percent / 100.0);
            segs.Add(new JsonObject
            {
                ["id"] = id++, ["start"] = first, ["stop"] = first + (swept.Count - 1) * length + size,
                ["grp"] = size, ["spc"] = length - size, ["on"] = true, ["bri"] = 255,
                ["fx"] = effectId(p["fx"]?.ToString() ?? "Solid"),
                ["pal"] = paletteId(p["pal"]?.ToString() ?? "Default"),
                ["sx"] = p["sx"]?.GetValue<int>() ?? 128,
                ["ix"] = p["ix"]?.GetValue<int>() ?? 128,
                ["col"] = col,
            });
        }
        if (swept.Count > 0)
        {
            for (var k = 0; k < half; k++)
            {
                var percent = endPercent + (100 - endPercent) * k / half;
                Band(k * width, width, percent);
                Band(length - (k + 1) * width, width, percent);
            }
            Band(half * width, length - 2 * half * width, 100);
        }
        // remove any overlay segments or bands left over from another look
        while (id < MaxSegments)
            segs.Add(new JsonObject { ["id"] = id++, ["stop"] = 0 });
        return new JsonObject { ["on"] = true, ["seg"] = segs };
    }

    static JsonObject Segment(StripSegment s, int id, string name, JsonObject p,
        Func<string, int> effectId, Func<string, int> paletteId)
    {
        var twist = p["twist"]?.GetValue<int>() ?? 0;
        var length = s.Stop - s.Start;
        var flip = Flip(p["flip"]?.ToString(), s.Strip);
        return new JsonObject
        {
            ["id"] = id,
            ["start"] = s.Start,
            ["stop"] = s.Stop,
            ["n"] = name,
            ["on"] = true,
            ["bri"] = 255,   // undoes a show's fade-down step
            ["fx"] = effectId(p["fx"]?.ToString() ?? "Solid"),
            ["pal"] = paletteId(p["pal"]?.ToString() ?? "Default"),
            ["sx"] = p["sx"]?.GetValue<int>() ?? 128,
            ["ix"] = p["ix"]?.GetValue<int>() ?? 128,
            ["col"] = Colours(p["colours"]),
            ["of"] = ((s.Strip - 1) * twist % length + length) % length,
            ["rev"] = s.RunsDown ^ flip,
            // undo what an "across" look leaves behind
            ["frz"] = false,
        };
    }

    /// <summary>The look's own settings, overlaid by every group the strip belongs to.</summary>
    static JsonObject Merge(JsonNode look, int strip, int stripCount)
    {
        var merged = new JsonObject();
        foreach (var (key, value) in look.AsObject())
            if (key is not ("groups" or "overlay" or "across")) merged[key] = value?.DeepClone();
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
        var preset = Playlist(ids, ids.Select(_ => tenths).ToList(), fade, 0);
        preset["psave"] = show["id"]!.GetValue<int>();
        preset["n"] = show["name"]!.ToString();
        preset["o"] = true; // store the playlist itself, not the current state
        return preset;
    }

    /// <summary>
    /// Rebuilds a stored show's playlist the current way (Fade down steps with the black gap), keeping its looks,
    /// times, fade, limit and ending. A show saved before fades existed gets its old transition as the fade.
    /// False if it isn't a show or is already up to date.
    /// </summary>
    public static bool AddFades(JsonObject preset)
    {
        if (preset["playlist"] is not JsonObject pl || pl["ps"] is not JsonArray ps || ps.Count == 0) return false;
        var raw = ps.Select(n => n!.GetValue<int>()).ToList();
        List<int> Values(JsonNode? v, int fallback) => v is JsonArray a
            ? a.Select(n => n!.GetValue<int>()).ToList()
            : raw.Select(_ => v?.GetValue<int>() ?? fallback).ToList();
        var rawDur = Values(pl["dur"], 100); var rawTr = Values(pl["transition"], 7);
        // back to looks and their times, as the app's showOf does
        var looks = new List<int>(); var dur = new List<int>(); var fade = rawTr[0];
        for (var i = 0; i < raw.Count; i++)
        {
            if (raw[i] == FadeDown) { if (dur.Count > 0) dur[^1] += rawDur[i]; fade = 2 * rawTr[i]; continue; }
            looks.Add(raw[i]); dur.Add(rawDur[i]);
        }
        if (looks.Count == 0 || fade < 2 || dur.Any(d => d < fade + FadeGap)) return false;
        var body = Playlist(looks, dur, fade, pl["repeat"]?.GetValue<int>() ?? 0);
        body["playlist"]!["end"] = pl["end"]?.GetValue<int>() ?? LightsOff;
        if (body["playlist"]!.ToJsonString() == pl.ToJsonString() && preset["transition"]?.ToString() == body["transition"]!.ToString())
            return false;
        preset["on"] = true;
        preset["transition"] = body["transition"]!.GetValue<int>();
        preset["playlist"] = body["playlist"]!.DeepClone();
        return true;
    }

    /// <summary>
    /// Tenths of a second at black between the fade-down and the next look. WLED 0.14 won't start a transition
    /// while one is running, so a look that arrives as the fade-down ends would skip its fade-up.
    /// </summary>
    public const int FadeGap = 2;

    /// <summary>
    /// The playlist body for looks shown for the given times (tenths of a second each) with a fade between
    /// them: half of it fading the look down (the FadeDown step, plus FadeGap at black), half fading the next one up. The show's
    /// "transition" sets the fade speed: WLED 0.14 uses it for the playlist's steps and ignores the per-step
    /// times. Must match playlistBody in app/tree.htm.
    /// </summary>
    public static JsonObject Playlist(List<int> looks, List<int> tenths, int fadeTenths, int repeat)
    {
        var half = fadeTenths / 2;
        var ps = new JsonArray(); var dur = new JsonArray(); var tr = new JsonArray();
        for (var i = 0; i < looks.Count; i++)
        {
            var gap = half > 0 ? FadeGap : 0;
            ps.Add(looks[i]); dur.Add(Math.Max(1, tenths[i] - half - gap)); tr.Add(half);
            if (half > 0) { ps.Add(FadeDown); dur.Add(half + gap); tr.Add(half); }
        }
        return new JsonObject
        {
            ["on"] = true,
            ["transition"] = half,
            ["playlist"] = new JsonObject
            {
                ["ps"] = ps, ["dur"] = dur, ["transition"] = tr,
                ["repeat"] = repeat, ["end"] = LightsOff, ["r"] = 0,
            },
        };
    }
}
