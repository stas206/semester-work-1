using System.Net;
using MiniHttpServer.Dao;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Models;
using MiniHttpServer.Validation;
using MiniHttpServer.ViewModels;

namespace MiniHttpServer.Controllers;

public abstract class PovelitelController : BaseController
{
    private static readonly string ImagesRoot =
        Path.Combine(AppContext.BaseDirectory, "static", "assets", "images");

    protected bool IsCurrentUserAdmin => IsAuthenticated();

    protected void ApplyBase(SitePageModel model)
    {
        model.IsAdmin = IsCurrentUserAdmin;
        model.CurrentYear = DateTime.Now.Year;
    }

    protected IHttpResult? RequireAdmin()
    {
        if (!IsCurrentUserAdmin)
            return Redirect("/auth/login?next=/admin/index");
        return null;
    }

    protected IHttpResult ErrorPage(int status, string message)
    {
        var model = new ErrorModel
        {
            StatusCode = status,
            Message = message,
            Title = status + " — Повелитель Токио"
        };
        ApplyBase(model);
        return Page("Shared/Error.html", model, (HttpStatusCode)status);
    }

    protected IReadOnlyList<string> ListAvailableImages()
    {
        var list = new List<string>();
        try
        {
            if (Directory.Exists(ImagesRoot))
            {
                foreach (var file in Directory.EnumerateFiles(ImagesRoot, "*.svg"))
                    list.Add("/assets/images/" + Path.GetFileName(file));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[PovelitelController] ListAvailableImages: " + ex.Message);
        }
        return list;
    }

    protected string? SaveUploadedImage(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl) || !dataUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return null;
        int comma = dataUrl.IndexOf(',');
        if (comma < 0) return null;
        string header = dataUrl.Substring(0, comma);
        string base64 = dataUrl.Substring(comma + 1);
        byte[] bytes;
        try { bytes = Convert.FromBase64String(base64); }
        catch { return null; }
        if (bytes.Length == 0 || bytes.Length > 3 * 1024 * 1024) return null;

        string ext = ".png";
        if (header.Contains("jpeg", StringComparison.OrdinalIgnoreCase) || header.Contains("jpg", StringComparison.OrdinalIgnoreCase)) ext = ".jpg";
        else if (header.Contains("svg", StringComparison.OrdinalIgnoreCase)) ext = ".svg";
        else if (header.Contains("gif", StringComparison.OrdinalIgnoreCase)) ext = ".gif";
        else if (header.Contains("webp", StringComparison.OrdinalIgnoreCase)) ext = ".webp";

        var dir = Path.Combine(ImagesRoot, "uploads");
        Directory.CreateDirectory(dir);
        string fileName = Guid.NewGuid().ToString("N") + ext;
        File.WriteAllBytes(Path.Combine(dir, fileName), bytes);
        return "/assets/images/uploads/" + fileName;
    }
}

[Controller]
public sealed class HomeController(IMonsterRepository monsters) : PovelitelController
{
    [Get]
    public async Task<IHttpResult> Index()
    {
        var model = new HomeModel
        {
            Title = "Повелитель Токио — настольная игра",
            FeaturedMonsters = (await monsters.GetAllAsync().ConfigureAwait(false)).Take(3).ToList()
        };
        ApplyBase(model);
        return Page("Home/Index.html", model);
    }
}

[Controller]
public sealed class CardController(ICardRepository cards) : PovelitelController
{
    [Get]
    public async Task<IHttpResult> Index(string? q)
    {
        var list = string.IsNullOrWhiteSpace(q)
            ? await cards.GetAllAsync().ConfigureAwait(false)
            : await cards.SearchAsync(q).ConfigureAwait(false);
        var model = new CardListModel { Title = "Карты — Повелитель Токио", Cards = list, Query = q ?? string.Empty };
        ApplyBase(model);
        return Page("Card/Index.html", model);
    }

    [Get]
    public async Task<IHttpResult> Details(int id)
    {
        var card = await cards.GetByIdAsync(id).ConfigureAwait(false);
        if (card == null)
            return ErrorPage(404, "Карта не найдена. Возможно, она была удалена.");
        var model = new CardDetailModel { Title = card.Name + " — Повелитель Токио", Card = card };
        ApplyBase(model);
        return Page("Card/Details.html", model);
    }

    [Get]
    public async Task<IHttpResult> Search(string? q)
    {
        var list = await cards.SearchAsync(q ?? string.Empty).ConfigureAwait(false);
        var data = list.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            type = c.Type,
            imagePath = c.ImagePath
        });
        return Json(new { ok = true, cards = data });
    }
}

[Controller]
public sealed class ItemController(IItemRepository items) : PovelitelController
{
    [Get]
    public async Task<IHttpResult> Index()
    {
        var list = await items.GetAllAsync().ConfigureAwait(false);
        var model = new ItemListModel { Title = "Предметы — Повелитель Токио", Items = list };
        ApplyBase(model);
        return Page("Item/Index.html", model);
    }
}

[Controller]
public sealed class RuleController(IRuleRepository rules) : PovelitelController
{
    [Get]
    public async Task<IHttpResult> Index()
    {
        var list = (await rules.GetAllAsync().ConfigureAwait(false))
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id).ToList();
        var model = new RuleListModel { Title = "Правила — Повелитель Токио", Rules = list };
        ApplyBase(model);
        return Page("Rule/Index.html", model);
    }

    [Get]
    public async Task<IHttpResult> Details(string slug)
    {
        var rule = await rules.GetBySlugAsync(slug ?? string.Empty).ConfigureAwait(false);
        if (rule == null)
            return ErrorPage(404, "Раздел правил не найден.");
        var all = (await rules.GetAllAsync().ConfigureAwait(false))
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id).ToList();
        var model = new RuleDetailModel { Title = rule.Title + " — Повелитель Токио", Rule = rule, Rules = all };
        ApplyBase(model);
        return Page("Rule/Details.html", model);
    }
}

[Controller]
public sealed class StrategyController(IStrategyRepository strategies) : PovelitelController
{
    [Get]
    public async Task<IHttpResult> Index()
    {
        var list = (await strategies.GetAllAsync().ConfigureAwait(false))
            .OrderBy(s => s.Category).ThenBy(s => s.Id).ToList();
        var model = new StrategyListModel { Title = "Стратегии — Повелитель Токио", Strategies = list };
        ApplyBase(model);
        return Page("Strategy/Index.html", model);
    }

    [Get]
    public async Task<IHttpResult> Details(string slug)
    {
        var strategy = await strategies.GetBySlugAsync(slug ?? string.Empty).ConfigureAwait(false);
        if (strategy == null)
            return ErrorPage(404, "Стратегия не найдена.");
        var all = (await strategies.GetAllAsync().ConfigureAwait(false))
            .OrderBy(s => s.Category).ThenBy(s => s.Id).ToList();
        var model = new StrategyDetailModel { Title = strategy.Title + " — Повелитель Токио", Strategy = strategy, Strategies = all };
        ApplyBase(model);
        return Page("Strategy/Details.html", model);
    }
}

[Controller]
public sealed class AuthController(IUserRepository users) : PovelitelController
{
    [Get]
    [Post]
    public async Task<IHttpResult> Login(string? username, string? password, string? next)
    {
        var target = string.IsNullOrWhiteSpace(next) ? "/admin/index" : next;

        if (!string.Equals(Context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            var model = new LoginModel { Title = "Авторизация — Повелитель Токио", Next = target };
            ApplyBase(model);
            return Page("Auth/Login.html", model);
        }

        var validation = Validator.ValidateLogin(username ?? string.Empty, password ?? string.Empty);
        if (!validation.IsValid)
            return LoginFailed(target, validation.ErrorHtml, username ?? string.Empty);

        var user = await users.GetByUsernameAsync(username ?? string.Empty).ConfigureAwait(false);
        if (user == null || !PasswordHasher.Verify(password ?? string.Empty, user.Salt, user.PasswordHash))
            return LoginFailed(target, "Неверный логин или пароль.", username ?? string.Empty);

        if (!string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase))
            return LoginFailed(target, "У учётной записи нет доступа к админ-панели.", username ?? string.Empty);

        var sessionId = SessionService.CreateSession(user.Id);
        var cookie = new Cookie("sessionId", sessionId, "/")
        {
            HttpOnly = true,
            Secure = Context.Request.IsSecureConnection,
            Expires = DateTime.UtcNow.AddHours(2)
        };
        Context.Response.SetCookie(cookie);
        return Redirect(target);
    }

    private IHttpResult LoginFailed(string next, string error, string username)
    {
        var model = new LoginModel { Title = "Авторизация — Повелитель Токио", Next = next, Error = error, Username = username };
        ApplyBase(model);
        return Page("Auth/Login.html", model);
    }

    [Get]
    public IHttpResult Logout()
    {
        SessionService.RemoveSession(SessionId);
        var cookie = new Cookie("sessionId", string.Empty, "/")
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        Context.Response.SetCookie(cookie);
        return Redirect("/home/index");
    }
}

[Controller]
public sealed class AdminController(
    IMonsterRepository monsters,
    ICardRepository cards,
    IItemRepository items,
    IRuleRepository rules,
    IStrategyRepository strategies) : PovelitelController
{
    [Get]
    public async Task<IHttpResult> Index()
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var model = new AdminModel
        {
            Title = "Админ-панель — Повелитель Токио",
            Monsters = await monsters.GetAllAsync().ConfigureAwait(false),
            Cards = await cards.GetAllAsync().ConfigureAwait(false),
            Items = await items.GetAllAsync().ConfigureAwait(false),
            Rules = await rules.GetAllAsync().ConfigureAwait(false),
            Strategies = await strategies.GetAllAsync().ConfigureAwait(false)
        };
        ApplyBase(model);
        return Page("Admin/Index.html", model);
    }

    [Get]
    public async Task<IHttpResult> CardForm(int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var card = id.HasValue && id.Value > 0
            ? await cards.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Card();
        if (card == null) card = new Card();

        var model = new CardFormModel
        {
            Title = (card.Id > 0 ? "Редактировать карту" : "Новая карта") + " — Повелитель Токио",
            Card = card,
            IsEdit = card.Id > 0,
            ImageOptions = ListAvailableImages()
        };
        ApplyBase(model);
        return Page("Admin/CardForm.html", model);
    }

    [Post]
    public async Task<IHttpResult> SaveCard(
        string? name, string? type, string? description, int? id,
        string? image_data, string? image_path)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var validation = Validator.ValidateCard(name, type, description);
        var card = id.HasValue && id.Value > 0
            ? await cards.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Card();
        if (card == null) card = new Card();

        card.Name = name ?? string.Empty;
        card.Type = type ?? string.Empty;
        card.Description = description ?? string.Empty;

        if (!validation.IsValid)
            return RenderCardForm(card, validation.ErrorHtml);

        card.ImagePath = ResolveImagePath(image_data, image_path, card.ImagePath);

        if (card.Id > 0) await cards.UpdateAsync(card).ConfigureAwait(false);
        else await cards.InsertAsync(card).ConfigureAwait(false);
        return Redirect("/admin/index");
    }

    [Post]
    public async Task<IHttpResult> DeleteCard(int id)
    {
        if (!IsCurrentUserAdmin)
            return Json(new { ok = false, error = "auth" });
        await cards.DeleteAsync(id).ConfigureAwait(false);
        return Json(new { ok = true });
    }

    [Get]
    public async Task<IHttpResult> ItemForm(int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var item = id.HasValue && id.Value > 0
            ? await items.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Item();
        if (item == null) item = new Item();

        var model = new ItemFormModel
        {
            Title = (item.Id > 0 ? "Редактировать предмет" : "Новый предмет") + " — Повелитель Токио",
            Item = item,
            IsEdit = item.Id > 0,
            ImageOptions = ListAvailableImages()
        };
        ApplyBase(model);
        return Page("Admin/ItemForm.html", model);
    }

    [Post]
    public async Task<IHttpResult> SaveItem(
        string? name, string? category, string? description,
        string? quantity, int? id, string? image_data, string? image_path)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var validation = Validator.ValidateItem(name, category, description, quantity);
        var item = id.HasValue && id.Value > 0
            ? await items.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Item();
        if (item == null) item = new Item();

        item.Name = name ?? string.Empty;
        item.Category = category ?? string.Empty;
        item.Description = description ?? string.Empty;
        item.Quantity = int.TryParse(quantity, out var q) ? q : 0;

        if (!validation.IsValid)
            return RenderItemForm(item, validation.ErrorHtml);

        item.ImagePath = ResolveImagePath(image_data, image_path, item.ImagePath);

        if (item.Id > 0) await items.UpdateAsync(item).ConfigureAwait(false);
        else await items.InsertAsync(item).ConfigureAwait(false);
        return Redirect("/admin/index");
    }

    [Post]
    public async Task<IHttpResult> DeleteItem(int id)
    {
        if (!IsCurrentUserAdmin)
            return Json(new { ok = false, error = "auth" });
        await items.DeleteAsync(id).ConfigureAwait(false);
        return Json(new { ok = true });
    }

    [Get]
    public async Task<IHttpResult> MonsterForm(int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var monster = id.HasValue && id.Value > 0
            ? await monsters.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Monster();
        if (monster == null) monster = new Monster();

        var model = new MonsterFormModel
        {
            Title = (monster.Id > 0 ? "Редактировать монстра" : "Новый монстр") + " — Повелитель Токио",
            Monster = monster,
            IsEdit = monster.Id > 0,
            ImageOptions = ListAvailableImages()
        };
        ApplyBase(model);
        return Page("Admin/MonsterForm.html", model);
    }

    [Post]
    public async Task<IHttpResult> SaveMonster(string? name, string? description, int? id, string? image_data, string? image_path)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var validation = Validator.ValidateMonster(name, description);
        var monster = id.HasValue && id.Value > 0
            ? await monsters.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Monster();
        if (monster == null) monster = new Monster();

        monster.Name = name ?? string.Empty;
        monster.Description = description ?? string.Empty;

        if (!validation.IsValid)
            return RenderMonsterForm(monster, validation.ErrorHtml);

        monster.ImagePath = ResolveImagePath(image_data, image_path, monster.ImagePath);

        if (monster.Id > 0) await monsters.UpdateAsync(monster).ConfigureAwait(false);
        else await monsters.InsertAsync(monster).ConfigureAwait(false);
        return Redirect("/admin/index");
    }

    [Post]
    public async Task<IHttpResult> DeleteMonster(int id)
    {
        if (!IsCurrentUserAdmin)
            return Json(new { ok = false, error = "auth" });
        await monsters.DeleteAsync(id).ConfigureAwait(false);
        return Json(new { ok = true });
    }

    [Post]
    public async Task<IHttpResult> DeleteRule(int id)
    {
        if (!IsCurrentUserAdmin)
            return Json(new { ok = false, error = "auth" });
        await rules.DeleteAsync(id).ConfigureAwait(false);
        return Json(new { ok = true });
    }

    [Post]
    public async Task<IHttpResult> DeleteStrategy(int id)
    {
        if (!IsCurrentUserAdmin)
            return Json(new { ok = false, error = "auth" });
        await strategies.DeleteAsync(id).ConfigureAwait(false);
        return Json(new { ok = true });
    }

    [Get]
    public async Task<IHttpResult> RuleForm(int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var rule = id.HasValue && id.Value > 0
            ? await rules.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Rule();
        if (rule == null) rule = new Rule();

        var model = new RuleFormModel
        {
            Title = (rule.Id > 0 ? "Редактировать раздел правил" : "Новый раздел правил") + " — Повелитель Токио",
            Rule = rule,
            IsEdit = rule.Id > 0
        };
        ApplyBase(model);
        return Page("Admin/RuleForm.html", model);
    }

    [Post]
    public async Task<IHttpResult> SaveRule(string? title, string? slug, string? content, int? sort_order, int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var validation = Validator.ValidateRule(title, slug, content);
        var rule = id.HasValue && id.Value > 0
            ? await rules.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Rule();
        if (rule == null) rule = new Rule();

        rule.Title = title ?? string.Empty;
        rule.Slug = slug ?? string.Empty;
        rule.Content = content ?? string.Empty;
        rule.SortOrder = sort_order ?? 0;

        if (!validation.IsValid)
            return RenderRuleForm(rule, validation.ErrorHtml);

        if (rule.Id > 0) await rules.UpdateAsync(rule).ConfigureAwait(false);
        else await rules.InsertAsync(rule).ConfigureAwait(false);
        return Redirect("/admin/index");
    }

    [Get]
    public async Task<IHttpResult> StrategyForm(int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var strategy = id.HasValue && id.Value > 0
            ? await strategies.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Strategy();
        if (strategy == null) strategy = new Strategy();

        var model = new StrategyFormModel
        {
            Title = (strategy.Id > 0 ? "Редактировать стратегию" : "Новая стратегия") + " — Повелитель Токио",
            Strategy = strategy,
            IsEdit = strategy.Id > 0
        };
        ApplyBase(model);
        return Page("Admin/StrategyForm.html", model);
    }

    [Post]
    public async Task<IHttpResult> SaveStrategy(string? title, string? slug, string? category, string? content, int? id)
    {
        var guard = RequireAdmin();
        if (guard != null) return guard;

        var validation = Validator.ValidateStrategy(title, slug, category, content);
        var strategy = id.HasValue && id.Value > 0
            ? await strategies.GetByIdAsync(id.Value).ConfigureAwait(false)
            : new Strategy();
        if (strategy == null) strategy = new Strategy();

        strategy.Title = title ?? string.Empty;
        strategy.Slug = slug ?? string.Empty;
        strategy.Category = category ?? string.Empty;
        strategy.Content = content ?? string.Empty;

        if (!validation.IsValid)
            return RenderStrategyForm(strategy, validation.ErrorHtml);

        if (strategy.Id > 0) await strategies.UpdateAsync(strategy).ConfigureAwait(false);
        else await strategies.InsertAsync(strategy).ConfigureAwait(false);
        return Redirect("/admin/index");
    }

    private IHttpResult RenderCardForm(Card card, string errorHtml)
    {
        var model = new CardFormModel { Title = "Карта — Повелитель Токио", Card = card, ErrorHtml = errorHtml, IsEdit = card.Id > 0, ImageOptions = ListAvailableImages() };
        ApplyBase(model);
        return Page("Admin/CardForm.html", model);
    }

    private IHttpResult RenderItemForm(Item item, string errorHtml)
    {
        var model = new ItemFormModel { Title = "Предмет — Повелитель Токио", Item = item, ErrorHtml = errorHtml, IsEdit = item.Id > 0, ImageOptions = ListAvailableImages() };
        ApplyBase(model);
        return Page("Admin/ItemForm.html", model);
    }

    private IHttpResult RenderMonsterForm(Monster monster, string errorHtml)
    {
        var model = new MonsterFormModel { Title = "Монстр — Повелитель Токио", Monster = monster, ErrorHtml = errorHtml, IsEdit = monster.Id > 0, ImageOptions = ListAvailableImages() };
        ApplyBase(model);
        return Page("Admin/MonsterForm.html", model);
    }

    private IHttpResult RenderRuleForm(Rule rule, string errorHtml)
    {
        var model = new RuleFormModel { Title = "Раздел правил — Повелитель Токио", Rule = rule, ErrorHtml = errorHtml, IsEdit = rule.Id > 0 };
        ApplyBase(model);
        return Page("Admin/RuleForm.html", model);
    }

    private IHttpResult RenderStrategyForm(Strategy strategy, string errorHtml)
    {
        var model = new StrategyFormModel { Title = "Стратегия — Повелитель Токио", Strategy = strategy, ErrorHtml = errorHtml, IsEdit = strategy.Id > 0 };
        ApplyBase(model);
        return Page("Admin/StrategyForm.html", model);
    }

    private string ResolveImagePath(string? imageData, string? imagePath, string current)
    {
        var saved = SaveUploadedImage(imageData);
        if (!string.IsNullOrEmpty(saved)) return saved;
        if (!string.IsNullOrWhiteSpace(imagePath)) return imagePath;
        return current ?? string.Empty;
    }
}
