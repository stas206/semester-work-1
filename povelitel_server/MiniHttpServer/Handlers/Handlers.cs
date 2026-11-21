using System.Net;
using System.Text;
using MiniHttpServer.Framework.Authentication;
using MiniHttpServer.Framework.Handlers;
using MiniHttpServer.HtmlTemplating;
using MiniHttpServer.ViewModels;

namespace MiniHttpServer.Handlers;

public sealed class RootRedirectHandler : RequestHandler
{
    public override Task HandleAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "/";
        if (string.Equals(path, "/", StringComparison.Ordinal))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Redirect;
            context.Response.RedirectLocation = "/home/index";
            context.Response.Close();
            return Task.CompletedTask;
        }
        return HandleNextAsync(context);
    }
}

public sealed class PovelitelNotFoundHandler : RequestHandler
{
    private readonly IHtmlTemplateRenderer _renderer;
    private readonly IUserSessionService _sessions;

    public PovelitelNotFoundHandler(IHtmlTemplateRenderer renderer, IUserSessionService sessions)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
    }

    public override async Task HandleAsync(HttpListenerContext context)
    {
        var sessionId = context.Request.Cookies["sessionId"]?.Value;
        var model = new ErrorModel
        {
            StatusCode = 404,
            Message = "Похоже, монстр разнёс эту страницу. Вернитесь на главную и продолжите путь к званию Повелителя Токио.",
            IsAdmin = _sessions.GetUserId(sessionId).HasValue,
            Title = "404 — Повелитель Токио"
        };

        string html;
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "Shared", "Error.html");
            html = _renderer.RenderFromFile(path, model);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[PovelitelNotFoundHandler] Ошибка рендеринга 404: " + ex.Message);
            html = "<!DOCTYPE html><meta charset=\"utf-8\"><h1>404</h1><p>" +
                   System.Net.WebUtility.HtmlEncode(model.Message) +
                   "</p><a href=\"/home/index\">На главную</a>";
        }

        var bytes = Encoding.UTF8.GetBytes(html);
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.NotFound;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        response.Headers["Server"] = "MiniHttpServer";
        if (!context.Request.HttpMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
        response.Close();
    }
}
