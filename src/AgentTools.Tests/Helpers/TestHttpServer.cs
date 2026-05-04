using System.Net;
using System.Text;

namespace RichStokoe.AgentTools.Tests.Helpers;

/// <summary>
/// Minimal in-process HTTP server for testing tools that accept URLs as parameters.
/// </summary>
internal sealed class TestHttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Queue<(int StatusCode, string ContentType, string Body)> _queue = new();
    private readonly object _lock = new();

    public string BaseUrl { get; }

    public TestHttpServer()
    {
        var port = FindFreePort();
        BaseUrl = $"http://127.0.0.1:{port}";
        _listener = new HttpListener();
        _listener.Prefixes.Add($"{BaseUrl}/");
        _listener.Start();
        _ = ServeAsync(_cts.Token);
    }

    public void EnqueueResponse(int statusCode, string contentType, string body)
    {
        lock (_lock) _queue.Enqueue((statusCode, contentType, body));
    }

    private async Task ServeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync().WaitAsync(ct);
                (int StatusCode, string ContentType, string Body) resp;
                lock (_lock)
                {
                    if (!_queue.TryDequeue(out resp))
                        resp = (500, "text/plain", "No response queued");
                }
                ctx.Response.StatusCode = resp.StatusCode;
                ctx.Response.ContentType = resp.ContentType;
                var bytes = Encoding.UTF8.GetBytes(resp.Body);
                ctx.Response.ContentLength64 = bytes.Length;
                await ctx.Response.OutputStream.WriteAsync(bytes, ct);
                ctx.Response.OutputStream.Close();
            }
            catch (OperationCanceledException) { break; }
            catch { /* listener stopped or individual request failed */ }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _listener.Close();
        _cts.Dispose();
    }

    private static int FindFreePort()
    {
        var l = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}
