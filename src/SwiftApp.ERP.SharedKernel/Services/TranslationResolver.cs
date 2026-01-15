using System.Globalization;

namespace SwiftApp.ERP.SharedKernel.Services;

/// <summary>
/// Resolves the best translation from a collection of translation entities
/// based on the current culture. Fallback order: current culture → "de" → any → default.
/// </summary>
public static class TranslationResolver
{
    public const string FallbackLocale = "de";

    public static readonly IReadOnlyList<string> SupportedLocales =
        ["de", "fr", "it", "en"];

    public static string Resolve<T>(
        IEnumerable<T>? translations,
        Func<T, string?> localeGetter,
        Func<T, string?> valueGetter,
        string defaultValue)
    {
        if (translations is null)
            return defaultValue;

        var items = translations.ToList();
        if (items.Count == 0)
            return defaultValue;

        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var byLocale = items
            .Where(t => localeGetter(t) is not null)
            .GroupBy(t => localeGetter(t)!)
            .ToDictionary(g => g.Key, g => g.First());

        // 1. Exact language match
        if (byLocale.TryGetValue(lang, out var found))
        {
            var value = valueGetter(found);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        // 2. Fallback to "de"
        if (lang != FallbackLocale && byLocale.TryGetValue(FallbackLocale, out found))
        {
            var value = valueGetter(found);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        // 3. Any available translation
        var any = items
            .Select(valueGetter)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        return any ?? defaultValue;
    }

    public static Dictionary<string, string> ToMap<T>(
        IEnumerable<T>? translations,
        Func<T, string?> localeGetter,
        Func<T, string?> valueGetter)
    {
        if (translations is null)
            return [];

        return translations
            .Where(t => localeGetter(t) is not null && valueGetter(t) is not null)
            .GroupBy(t => localeGetter(t)!)
            .ToDictionary(g => g.Key, g => valueGetter(g.First())!);
    }
}
