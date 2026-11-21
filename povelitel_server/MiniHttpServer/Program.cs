using System.Reflection;
using MiniHttpServer.Dao;
using MiniHttpServer.Framework;
using MiniHttpServer.Framework.Authentication;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Handlers;
using MiniHttpServer.Framework.Routing;
using MiniHttpServer.Handlers;
using MiniHttpServer.HtmlTemplating;
using MiniHttpServer.ORM;
using MiniHttpServer.Services.Authentication;

var settings = ConfigurationManager.Instance.Settings;

var factory = new DefaultControllerFactory();
var orm = new OrmContext(settings.ConnectionString);
factory.Register<IOrmContext>(() => orm);
factory.Register<IMonsterRepository>(() => new MonsterRepository(orm));
factory.Register<ICardRepository>(() => new CardRepository(orm));
factory.Register<IItemRepository>(() => new ItemRepository(orm));
factory.Register<IRuleRepository>(() => new RuleRepository(orm));
factory.Register<IStrategyRepository>(() => new StrategyRepository(orm));
factory.Register<IUserRepository>(() => new UserRepository(orm));

IUserSessionService sessionService = new UserSessionService();
var templateRenderer = new HtmlTemplateRenderer();

var rootRedirect = new RootRedirectHandler();
var staticHandler = new StaticFilesHandler(settings.GetStaticRoot(), settings.AllowedSites, settings.Server.Path);
var controllersHandler = new ControllersHandler(
    Assembly.GetExecutingAssembly(), factory, templateRenderer, sessionService, settings.Server.Path);
var notFoundHandler = new PovelitelNotFoundHandler(templateRenderer, sessionService);

rootRedirect.SetNext(staticHandler);
staticHandler.SetNext(controllersHandler);
controllersHandler.SetNext(notFoundHandler);

var loggingHandler = new LoggingHandler();
loggingHandler.SetNext(rootRedirect);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var server = new HttpServer(settings, loggingHandler);
var serverTask = server.StartAsync();

Console.WriteLine("Сервер «Повелитель Токио» запущен. Введите stop для остановки.");

while (!cts.IsCancellationRequested)
{
    var command = await Task.Run(() => Console.ReadLine(), cts.Token).ConfigureAwait(false);
    if (command is null)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        break;
    }

    if (string.Equals(command.Trim(), "stop", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
}

server.Stop();
await serverTask.ConfigureAwait(false);