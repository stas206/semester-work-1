using System.Net;

namespace MiniHttpServer.Framework.Results;

public sealed class RedirectResult : IHttpResult
{
    private readonly string _location;
    private readonly HttpStatusCode _statusCode;

    public RedirectResult(string location, HttpStatusCode statusCode = HttpStatusCode.Redirect)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Адрес перенаправления не задан.", nameof(location));
        _location = location;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(
        HttpListenerResponse response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        response.StatusCode = (int)_statusCode;
        response.RedirectLocation = _location;
        response.Headers["Server"] = "MiniHttpServer";
        response.ContentLength64 = 0;
        await Task.CompletedTask;
    }
}
