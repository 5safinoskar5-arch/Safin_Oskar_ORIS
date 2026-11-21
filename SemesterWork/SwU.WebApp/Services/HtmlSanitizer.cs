using System.Text.RegularExpressions;

namespace SwU.WebApp.Services;

public class HtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "b", "i", "u", "strong", "em", "ul", "ol", "li", "span"
    };

    // Опасные парные теги удаляем вместе с содержимым
    private static readonly Regex DangerousBlocks = new(
        @"<\s*(script|style|iframe|object)\b[^>]*>.*?<\s*/\s*\1\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    // Открывающие/одиночные опасные теги удаляем отдельно
    private static readonly Regex DangerousTags = new(
        @"<\s*/?\s*(script|style|iframe|object)\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Обработчики событий вроде onclick="..." или onerror=alert(1)
    private static readonly Regex EventAttributes = new(
        @"\s+on[a-z0-9_-]+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex JavascriptUrl = new(
        @"javascript\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HtmlComments = new(
        @"<!--.*?-->",
        RegexOptions.Singleline | RegexOptions.Compiled);

    // Захватываем косую черту и имя тега, чтобы оставить только разрешенные теги
    private static readonly Regex TagRegex = new(
        @"<\s*(/?)\s*([a-zA-Z][a-zA-Z0-9]*)\b[^>]*>",
        RegexOptions.Compiled);

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        string result = html.Replace("\0", string.Empty);

        // Повторяем очистку до стабилизации, чтобы вложенные конструкции вроде <scr<script>ipt> не обходили фильтр
        string previous;
        int guard = 0;
        do
        {
            previous = result;
            result = HtmlComments.Replace(result, string.Empty);
            result = DangerousBlocks.Replace(result, string.Empty);
            result = DangerousTags.Replace(result, string.Empty);
            result = EventAttributes.Replace(result, string.Empty);
            result = JavascriptUrl.Replace(result, string.Empty);
            result = TagRegex.Replace(result, KeepAllowedTag);
            guard++;
        }
        while (!string.Equals(result, previous, StringComparison.Ordinal) && guard < 10);

        return result.Trim();
    }

    private static string KeepAllowedTag(Match match)
    {
        string tagName = match.Groups[2].Value.ToLowerInvariant();
        if (!AllowedTags.Contains(tagName))
        {
            return string.Empty;
        }

        bool isClosing = match.Groups[1].Value == "/";
        return isClosing ? $"</{tagName}>" : $"<{tagName}>";
    }
}
