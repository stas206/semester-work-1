using System.Net;
using System.Text.RegularExpressions;

namespace MiniHttpServer.Validation;

public sealed class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public string ErrorHtml { get; set; } = string.Empty;
}

public static class Validator
{
    public static readonly Regex NameRegex = new(@"^[A-Za-zА-Яа-яЁё0-9\s\-']{2,60}$", RegexOptions.Compiled);
    public static readonly Regex NumericRegex = new(@"^\d{1,3}$", RegexOptions.Compiled);
    public static readonly Regex SlugRegex = new(@"^[a-z0-9\-]{2,80}$", RegexOptions.Compiled);
    public static readonly Regex UsernameRegex = new(@"^[A-Za-z0-9_]{3,30}$", RegexOptions.Compiled);

    public static ValidationResult ValidateCard(string name, string type, string description)
    {
        var errors = new List<string>();
        if (!NameRegex.IsMatch(name ?? string.Empty))
            errors.Add("Название карты: 2–60 символов (буквы, цифры, пробел, дефис, апостроф).");
        if (string.IsNullOrWhiteSpace(type))
            errors.Add("Выберите тип карты.");
        if (string.IsNullOrEmpty(description) || description.Length < 10 || description.Length > 250)
            errors.Add("Описание карты: 10–250 символов.");
        return Build(errors);
    }

    public static ValidationResult ValidateMonster(string name, string description)
    {
        var errors = new List<string>();
        if (!NameRegex.IsMatch(name ?? string.Empty))
            errors.Add("Название монстра: 2–60 символов (буквы, цифры, пробел, дефис, апостроф).");
        if (string.IsNullOrEmpty(description) || description.Length < 10 || description.Length > 500)
            errors.Add("Описание монстра: 10–500 символов.");
        return Build(errors);
    }

    public static ValidationResult ValidateItem(string name, string category, string description, string quantity)
    {
        var errors = new List<string>();
        if (!NameRegex.IsMatch(name ?? string.Empty))
            errors.Add("Название предмета: 2–60 символов (буквы, цифры, пробел, дефис, апостроф).");
        if (string.IsNullOrWhiteSpace(category))
            errors.Add("Выберите категорию предмета.");
        if (string.IsNullOrEmpty(description) || description.Length < 10 || description.Length > 250)
            errors.Add("Описание предмета: 10–250 символов.");
        if (!NumericRegex.IsMatch(quantity ?? string.Empty))
            errors.Add("Количество: целое число 0–999.");
        return Build(errors);
    }

    public static ValidationResult ValidateRule(string title, string slug, string content)
    {
        var errors = new List<string>();
        if (!NameRegex.IsMatch(title ?? string.Empty))
            errors.Add("Заголовок раздела правил: 2–60 символов.");
        if (!SlugRegex.IsMatch(slug ?? string.Empty))
            errors.Add("Slug: 2–80 символов (латиница в нижнем регистре, цифры, дефис).");
        if (string.IsNullOrEmpty(content) || content.Length < 10 || content.Length > 8000)
            errors.Add("Содержимое раздела: 10–8000 символов.");
        return Build(errors);
    }

    public static ValidationResult ValidateStrategy(string title, string slug, string category, string content)
    {
        var errors = new List<string>();
        if (!NameRegex.IsMatch(title ?? string.Empty))
            errors.Add("Заголовок стратегии: 2–60 символов.");
        if (!SlugRegex.IsMatch(slug ?? string.Empty))
            errors.Add("Slug: 2–80 символов (латиница в нижнем регистре, цифры, дефис).");
        if (string.IsNullOrWhiteSpace(category))
            errors.Add("Выберите категорию стратегии.");
        if (string.IsNullOrEmpty(content) || content.Length < 10 || content.Length > 8000)
            errors.Add("Содержимое стратегии: 10–8000 символов.");
        return Build(errors);
    }

    public static ValidationResult ValidateLogin(string username, string password)
    {
        var errors = new List<string>();
        if (!UsernameRegex.IsMatch(username ?? string.Empty))
            errors.Add("Логин: 3–30 символов (латиница, цифры, подчёркивание).");
        if (string.IsNullOrEmpty(password) || password.Length < 6 || password.Length > 100)
            errors.Add("Пароль: 6–100 символов.");
        return Build(errors);
    }

    private static ValidationResult Build(List<string> errors)
    {
        var result = new ValidationResult { IsValid = errors.Count == 0 };
        if (errors.Count > 0)
        {
            var items = string.Join("", errors.Select(e => "<li>" + WebUtility.HtmlEncode(e) + "</li>"));
            result.ErrorHtml = "<div class=\"form-errors\"><strong>Исправьте ошибки:</strong><ul>" + items + "</ul></div>";
        }
        return result;
    }
}
