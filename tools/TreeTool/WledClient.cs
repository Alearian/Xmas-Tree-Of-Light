using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace TreeTool;

/// <summary>Thin wrapper over the WLED HTTP/JSON API.</summary>
public sealed class WledClient(string host) : IDisposable
{
    readonly HttpClient http = new() { BaseAddress = new Uri($"http://{host}/"), Timeout = TimeSpan.FromSeconds(10) };

    public string Host => host;

    public async Task<JsonNode> GetJsonAsync(string path)
    {
        var text = await http.GetStringAsync(path);
        return JsonNode.Parse(text) ?? throw new InvalidDataException($"{path} returned no JSON");
    }

    public async Task<string> GetStringAsync(string path) => await http.GetStringAsync(path);

    public async Task<JsonNode?> PostJsonAsync(string path, JsonNode body)
    {
        using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(path, content);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text);
    }

    /// <summary>Uploads a file to WLED's filesystem (same endpoint as the /edit page).</summary>
    public async Task UploadAsync(string localPath, string remoteName)
    {
        using var form = new MultipartFormDataContent();
        using var file = new StreamContent(File.OpenRead(localPath));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(file, "data", "/" + remoteName.TrimStart('/'));
        using var response = await http.PostAsync("upload", form);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var response = await http.GetAsync("json/info", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>Waits for the controller to answer again, e.g. after a config change reboots it.</summary>
    public async Task<bool> WaitOnlineAsync(TimeSpan timeout)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (await IsOnlineAsync()) return true;
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        return false;
    }

    public void Dispose() => http.Dispose();
}
