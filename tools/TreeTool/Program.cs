using System.Text.Json;
using System.Text.Json.Nodes;
using TreeTool;

const string Usage = """
treetool - configure the Xmas tree's WLED controller(s)

Usage: treetool <command> [--layout config/tree.json] [--yes]

Commands
  info            Show firmware, LED count and uptime for each controller
  backup          Save /json/cfg and /presets.json to device-backups/
  plan            Show what 'apply' would change (read-only)
  apply           Backup, then set LED outputs, current limiter, one segment per strip,
                  and save them as the layout preset (asks for confirmation)
  upload          Upload the phone app (app/tree.htm) and the layout (as tree.json)
  test <mode>     strips  - each strip a different colour (check numbering/mapping)
                  chase   - chase effect on every strip (check up/down direction)
                  white   - full white on every strip (for current measurement;
                            WLED's limiter still applies)
                  off     - turn the lights off
  serve [--port 8080]
                  Serve the phone app locally and proxy /json to the first controller,
                  for developing the app without uploading it each time
""";

var argsList = args.ToList();
if (argsList.Count == 0 || argsList[0] is "-h" or "--help" or "help")
{
    Console.WriteLine(Usage);
    return 0;
}

var root = FindRepoRoot();
var layoutPath = Path.GetFullPath(Path.Combine(root, Option("--layout") ?? "config/tree.json"));
var assumeYes = argsList.Contains("--yes");

try
{
    var layout = TreeLayout.Load(layoutPath);
    Console.WriteLine($"Layout: {layout.Name} ({Path.GetRelativePath(root, layoutPath)})");
    Console.WriteLine();

    return argsList[0] switch
    {
        "info" => await Info(layout),
        "backup" => await BackupAll(layout),
        "plan" => await Plan(layout),
        "apply" => await Apply(layout),
        "upload" => await Upload(layout),
        "test" => await Test(layout, argsList.ElementAtOrDefault(1) ?? ""),
        "serve" => await DevServer.Run(root, layoutPath, layout, int.Parse(Option("--port") ?? "8080")),
        _ => Fail($"Unknown command '{argsList[0]}'.\n\n{Usage}"),
    };
}
catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
{
    return Fail($"Could not reach the controller: {e.Message}\nIs the PC connected to the tree's Wi-Fi?");
}
catch (Exception e) when (e is InvalidDataException or JsonException or FileNotFoundException)
{
    return Fail(e.Message);
}

async Task<int> Info(TreeLayout layout)
{
    foreach (var c in layout.Controllers)
    {
        using var wled = new WledClient(c.Host);
        var info = await wled.GetJsonAsync("json/info");
        Console.WriteLine($"{c.Name} ({c.Host}): WLED {info["ver"]} '{info["name"]}', " +
            $"{info["leds"]?["count"]} LEDs, uptime {info["uptime"]} s, free heap {info["freeheap"]} bytes, " +
            $"filesystem {info["fs"]?["u"]}/{info["fs"]?["t"]} KB");
    }
    return 0;
}

async Task<int> BackupAll(TreeLayout layout)
{
    foreach (var c in layout.Controllers)
    {
        using var wled = new WledClient(c.Host);
        await Backup(wled, c);
    }
    return 0;
}

async Task Backup(WledClient wled, Controller c)
{
    var dir = Path.Combine(root, "device-backups", $"{Safe(c.Name)}-{DateTime.Now:yyyyMMdd-HHmmss}");
    Directory.CreateDirectory(dir);
    var pretty = new JsonSerializerOptions { WriteIndented = true };
    await File.WriteAllTextAsync(Path.Combine(dir, "cfg.json"), (await wled.GetJsonAsync("json/cfg")).ToJsonString(pretty));
    await File.WriteAllTextAsync(Path.Combine(dir, "state.json"), (await wled.GetJsonAsync("json/state")).ToJsonString(pretty));
    try
    {
        await File.WriteAllTextAsync(Path.Combine(dir, "presets.json"), await wled.GetStringAsync("presets.json"));
    }
    catch (HttpRequestException)
    {
        // A board with no saved presets has no presets.json.
    }
    Console.WriteLine($"Backed up {c.Name} to {Path.GetRelativePath(root, dir)}");
}

async Task<int> Plan(TreeLayout layout)
{
    foreach (var c in layout.Controllers)
    {
        using var wled = new WledClient(c.Host);
        Planner.PrintPlan(layout, c, await wled.GetJsonAsync("json/cfg"), await wled.GetJsonAsync("json/state"));
    }
    return 0;
}

async Task<int> Apply(TreeLayout layout)
{
    foreach (var c in layout.Controllers)
    {
        using var wled = new WledClient(c.Host);
        await Backup(wled, c);
        var cfg = await wled.GetJsonAsync("json/cfg");
        Planner.PrintPlan(layout, c, cfg, await wled.GetJsonAsync("json/state"));
        var cfgMatches = Planner.CfgMatches(layout, c, cfg);

        var question = cfgMatches
            ? $"LED outputs already match. Rebuild the strip segments on {c.Name}?"
            : $"Apply this to {c.Name}? The controller will reboot.";
        if (!assumeYes && !Confirm(question))
        {
            Console.WriteLine("Skipped.");
            continue;
        }

        if (!cfgMatches)
        {
            Console.WriteLine("Writing LED output config...");
            await wled.PostJsonAsync("json/cfg", Planner.DesiredCfg(layout, c));
            await wled.PostJsonAsync("json/state", new JsonObject { ["rb"] = true });
            Console.WriteLine("Rebooting...");
            // WLED tries the home Wi-Fi before opening its AP, and the PC then has to rejoin, so allow a while.
            if (!await wled.WaitOnlineAsync(TimeSpan.FromSeconds(150)))
                return Fail($"{c.Name} did not come back within 150 s. Reconnect the PC to the tree's Wi-Fi and re-run apply (it is safe to repeat); the backup is in device-backups/.");
        }

        var info = await wled.GetJsonAsync("json/info");
        var maxSeg = info["leds"]?["maxseg"]?.GetValue<int>() ?? 32;
        Console.WriteLine("Creating one segment per strip...");
        await wled.PostJsonAsync("json/state", Planner.DesiredSegmentsState(layout, c, maxSeg));
        await wled.PostJsonAsync("json/state", new JsonObject
        {
            ["psave"] = layout.LayoutPreset,
            ["n"] = "Tree layout",
            ["ib"] = true,
            ["sb"] = true,
        });

        var count = (await wled.GetJsonAsync("json/info"))["leds"]?["count"];
        Console.WriteLine($"Done: {c.Name} now reports {count} LEDs; layout saved as preset {layout.LayoutPreset} (boot preset).");
    }
    return 0;
}

async Task<int> Upload(TreeLayout layout)
{
    var app = Path.Combine(root, "app", "tree.htm");
    foreach (var c in layout.Controllers)
    {
        using var wled = new WledClient(c.Host);
        await wled.UploadAsync(app, "tree.htm");
        await wled.UploadAsync(layoutPath, "tree.json");
        Console.WriteLine($"Uploaded to {c.Name}: open http://{c.Host}/tree.htm on your phone");
    }
    return 0;
}

async Task<int> Test(TreeLayout layout, string mode)
{
    foreach (var c in layout.Controllers)
    {
        using var wled = new WledClient(c.Host);
        var segments = LayoutMath.Segments(layout, c);
        JsonObject body;
        switch (mode)
        {
            case "strips":
                body = SegmentsBody(segments, s => new JsonObject { ["fx"] = 0, ["col"] = Colours(Hue(s.Strip, layout.StripCount)) });
                Console.WriteLine("Each strip should show its own colour. Strip 1 is red, then moving round the colour wheel clockwise.");
                break;
            case "chase":
                var fx = await EffectId(wled, "Chase");
                body = SegmentsBody(segments, s => new JsonObject { ["fx"] = fx, ["sx"] = 128, ["col"] = Colours([255, 255, 255]) });
                Console.WriteLine("Every strip should chase in the SAME direction: bottom to top.");
                break;
            case "white":
                body = SegmentsBody(segments, s => new JsonObject { ["fx"] = 0, ["col"] = Colours([255, 255, 255]) });
                body["bri"] = 255;
                Console.WriteLine($"Full white requested. WLED's limiter caps it at {layout.Abl.MaxMa} mA; raise abl.maxMa in the layout (or turn it off in WLED) to measure the true draw.");
                break;
            case "off":
                body = new JsonObject { ["on"] = false };
                break;
            default:
                return Fail("test needs a mode: strips | chase | white | off");
        }
        await wled.PostJsonAsync("json/state", body);
    }
    return 0;
}

static JsonObject SegmentsBody(List<StripSegment> segments, Func<StripSegment, JsonObject> seg)
{
    var array = new JsonArray();
    foreach (var s in segments)
    {
        var o = seg(s);
        o["id"] = s.SegmentId;
        o["on"] = true;
        array.Add(o);
    }
    return new JsonObject { ["on"] = true, ["seg"] = array };
}

static JsonArray Colours(int[] rgb) => new(new JsonArray(rgb[0], rgb[1], rgb[2]), new JsonArray(0, 0, 0), new JsonArray(0, 0, 0));

static int[] Hue(int strip, int count)
{
    var h = (strip - 1) * 6.0 / count;
    var x = (int)(255 * (1 - Math.Abs(h % 2 - 1)));
    return (int)h switch
    {
        0 => [255, x, 0],
        1 => [x, 255, 0],
        2 => [0, 255, x],
        3 => [0, x, 255],
        4 => [x, 0, 255],
        _ => [255, 0, x],
    };
}

static async Task<int> EffectId(WledClient wled, string name)
{
    var effects = (await wled.GetJsonAsync("json/eff")).AsArray();
    for (var i = 0; i < effects.Count; i++)
        if (string.Equals(effects[i]?.ToString(), name, StringComparison.OrdinalIgnoreCase))
            return i;
    throw new InvalidDataException($"Effect '{name}' not found on {wled.Host}");
}

static bool Confirm(string question)
{
    Console.Write($"{question} [y/N] ");
    return Console.ReadLine()?.Trim().ToLowerInvariant() is "y" or "yes";
}

static string Safe(string name) => string.Concat(name.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-'));

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}

string? Option(string name)
{
    var i = argsList.IndexOf(name);
    return i >= 0 && i + 1 < argsList.Count ? argsList[i + 1] : null;
}

static string FindRepoRoot()
{
    for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        if (File.Exists(Path.Combine(dir.FullName, "config", "tree.json")))
            return dir.FullName;
    throw new DirectoryNotFoundException("Run treetool from inside the Xmas repo (no config/tree.json found).");
}
