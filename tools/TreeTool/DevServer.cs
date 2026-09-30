using System.Net;

namespace TreeTool;

/// <summary>
/// Local dev server: serves app/tree.htm and the layout, and proxies everything else
/// (/json/*, /presets.json) to the first controller, so the app runs same-origin as on the board.
/// </summary>
public static class DevServer
{
    public static async Task<int> Run(string root, string layoutPath, TreeLayout layout, int port)
    {
        var target = layout.Controllers[0].Host;
        using var proxy = new HttpClient { BaseAddress = new Uri($"http://{target}/"), Timeout = TimeSpan.FromSeconds(10) };
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        Console.WriteLine($"Serving http://localhost:{port}/tree.htm (proxying to {target}). Ctrl+C to stop.");

        while (true)
        {
            var ctx = await listener.GetContextAsync();
            _ = Task.Run(() => Handle(ctx, root, layoutPath, proxy));
        }
    }

    static async Task Handle(HttpListenerContext ctx, string root, string layoutPath, HttpClient proxy)
    {
        var path = ctx.Request.Url!.AbsolutePath;
        try
        {
            if (path is "/" or "/tree.htm")
                await SendFile(ctx, Path.Combine(root, "app", "tree.htm"), "text/html; charset=utf-8");
            else if (path == "/tree.json")
                await SendFile(ctx, layoutPath, "application/json");
            else
                await Proxy(ctx, proxy);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = 502;
            await using var w = new StreamWriter(ctx.Response.OutputStream);
            await w.WriteAsync(e.Message);
        }
        finally
        {
            ctx.Response.Close();
        }
    }

    static async Task SendFile(HttpListenerContext ctx, string file, string type)
    {
        ctx.Response.ContentType = type;
        ctx.Response.Headers["Cache-Control"] = "no-store";
        var bytes = await File.ReadAllBytesAsync(file);
        await ctx.Response.OutputStream.WriteAsync(bytes);
    }

    static async Task Proxy(HttpListenerContext ctx, HttpClient proxy)
    {
        using var request = new HttpRequestMessage(new HttpMethod(ctx.Request.HttpMethod), ctx.Request.Url!.PathAndQuery.TrimStart('/'));
        if (ctx.Request.HasEntityBody)
        {
            using var body = new MemoryStream();
            await ctx.Request.InputStream.CopyToAsync(body);
            request.Content = new ByteArrayContent(body.ToArray());
            request.Content.Headers.TryAddWithoutValidation("Content-Type", ctx.Request.ContentType ?? "application/json");
        }
        using var response = await proxy.SendAsync(request);
        ctx.Response.StatusCode = (int)response.StatusCode;
        ctx.Response.ContentType = response.Content.Headers.ContentType?.ToString();
        await response.Content.CopyToAsync(ctx.Response.OutputStream);
    }
}
