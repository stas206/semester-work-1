using MiniHttpServer.Models;

namespace MiniHttpServer.ViewModels;

public class SitePageModel
{
    public string Title { get; set; } = "Повелитель Токио";
    public bool IsAdmin { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class HomeModel : SitePageModel
{
}

public class CardListModel : SitePageModel
{
    public IReadOnlyList<Card> Cards { get; set; } = Array.Empty<Card>();
    public string Query { get; set; } = string.Empty;
}

public class ItemListModel : SitePageModel
{
    public IReadOnlyList<Item> Items { get; set; } = Array.Empty<Item>();
}

public class RuleListModel : SitePageModel
{
    public IReadOnlyList<Rule> Rules { get; set; } = Array.Empty<Rule>();
}

public class StrategyListModel : SitePageModel
{
    public IReadOnlyList<Strategy> Strategies { get; set; } = Array.Empty<Strategy>();
}

public class LoginModel : SitePageModel
{
    public string Next { get; set; } = "/admin/index";
    public string Error { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class AdminModel : SitePageModel
{
    public IReadOnlyList<Card> Cards { get; set; } = Array.Empty<Card>();
    public IReadOnlyList<Item> Items { get; set; } = Array.Empty<Item>();
    public IReadOnlyList<Rule> Rules { get; set; } = Array.Empty<Rule>();
    public IReadOnlyList<Strategy> Strategies { get; set; } = Array.Empty<Strategy>();
}

public class CardFormModel : SitePageModel
{
    public Card Card { get; set; } = new();
    public string ErrorHtml { get; set; } = string.Empty;
    public bool IsEdit { get; set; }
}

public class ItemFormModel : SitePageModel
{
    public Item Item { get; set; } = new();
    public string ErrorHtml { get; set; } = string.Empty;
    public bool IsEdit { get; set; }
}

public class RuleFormModel : SitePageModel
{
    public Rule Rule { get; set; } = new();
    public string ErrorHtml { get; set; } = string.Empty;
    public bool IsEdit { get; set; }
}

public class StrategyFormModel : SitePageModel
{
    public Strategy Strategy { get; set; } = new();
    public string ErrorHtml { get; set; } = string.Empty;
    public bool IsEdit { get; set; }
}

public class ErrorModel : SitePageModel
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
