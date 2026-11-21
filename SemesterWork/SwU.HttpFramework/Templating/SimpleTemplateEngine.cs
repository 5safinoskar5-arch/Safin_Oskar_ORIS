using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SwU.HttpFramework.Templating;

public class SimpleTemplateEngine : ITemplateEngine
{
    // Находит самые внутренние блоки {{#each Items}}...{{/each}} без вложенных #each
    private static readonly Regex EachRegex = new(
        @"\{\{#each\s+([A-Za-z0-9_.]+)\s*\}\}((?:(?!\{\{#each\b|\{\{/each\}\}).)*?)\{\{/each\}\}",
        RegexOptions.Singleline | RegexOptions.Compiled);

    // Находит самые внутренние блоки {{#if Cond}}...{{else}}...{{/if}} без вложенных #if
    private static readonly Regex IfRegex = new(
        @"\{\{#if\s+([A-Za-z0-9_.]+)\s*\}\}((?:(?!\{\{#if\b|\{\{/if\}\}).)*?)(?:\{\{else\}\}((?:(?!\{\{#if\b|\{\{/if\}\}).)*?))?\{\{/if\}\}",
        RegexOptions.Singleline | RegexOptions.Compiled);

    // Один проход для подстановки {{{Raw}}} и {{Encoded}}, чтобы данные пользователя не перепарсивались как шаблон
    private static readonly Regex VariableRegex = new(
        @"\{\{\{\s*([A-Za-z0-9_.]+)\s*\}\}\}|\{\{\s*([A-Za-z0-9_.]+)\s*\}\}",
        RegexOptions.Compiled);

    private readonly string _viewsRootPath;

    public SimpleTemplateEngine(string viewsRootPath = "Views")
    {
        _viewsRootPath = Path.GetFullPath(viewsRootPath);
    }

    public string Render(string template, object? model)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        // Храним готовые фрагменты из {{#each}} по уникальному маркеру, чтобы данные из БД не перепарсивались
        string slotToken = Guid.NewGuid().ToString("N");
        List<string> renderedLoopSlots = new();
        string withLoops = ProcessEachBlocks(template, model, renderedLoopSlots, slotToken);
        string withConditions = ProcessIfBlocks(withLoops, model);
        string withVariables = ReplaceVariablesSinglePass(withConditions, model);

        return RestoreLoopSlots(withVariables, renderedLoopSlots, slotToken);
    }

    public string RenderFile(string viewName, object? model, string? layoutName = "layout.html")
    {
        string viewTemplate = LoadTemplateFile(viewName);
        string bodyHtml = Render(viewTemplate, model);

        if (string.IsNullOrWhiteSpace(layoutName))
        {
            return bodyHtml;
        }

        string layoutTemplate = LoadTemplateFile(layoutName);
        string layoutPlaceholder = $"__SWU_BODY_{Guid.NewGuid():N}__";
        Dictionary<string, object?> layoutModel = BuildLayoutModel(model, layoutPlaceholder);
        string renderedLayout = Render(layoutTemplate, layoutModel);
        return renderedLayout.Replace(layoutPlaceholder, bodyHtml, StringComparison.Ordinal);
    }

    private string LoadTemplateFile(string templateName)
    {
        string normalized = templateName.Replace('\\', '/').TrimStart('/');
        string fullPath = Path.GetFullPath(normalized, _viewsRootPath);
        string expectedPrefix = _viewsRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Недопустимый путь к файлу шаблона.");
        }

        return File.ReadAllText(fullPath, Encoding.UTF8);
    }

    private static Dictionary<string, object?> BuildLayoutModel(object? model, string bodyHtml)
    {
        Dictionary<string, object?> combined = new(StringComparer.OrdinalIgnoreCase);

        if (model is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                string? key = entry.Key?.ToString();
                if (!string.IsNullOrEmpty(key))
                {
                    combined[key] = entry.Value;
                }
            }
        }
        else if (model != null)
        {
            PropertyInfo[] properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetIndexParameters().Length == 0)
                {
                    combined[prop.Name] = prop.GetValue(model);
                }
            }
        }

        combined["Body"] = bodyHtml;
        return combined;
    }

    private string ProcessEachBlocks(
        string template,
        object? model,
        List<string> renderedLoopSlots,
        string slotToken)
    {
        string current = template;

        while (EachRegex.IsMatch(current))
        {
            current = EachRegex.Replace(current, match =>
            {
                string path = match.Groups[1].Value;
                string innerTemplate = match.Groups[2].Value;
                object? collectionObj = ResolveValue(model, path);

                if (collectionObj is not IEnumerable items || collectionObj is string)
                {
                    return string.Empty;
                }

                StringBuilder builder = new();
                foreach (object? item in items)
                {
                    string itemRendered = Render(innerTemplate, item);
                    builder.Append(itemRendered);
                }

                int slotIndex = renderedLoopSlots.Count;
                renderedLoopSlots.Add(builder.ToString());
                return $"__SWU_LOOP_{slotToken}_{slotIndex}__";
            });
        }

        return current;
    }

    private static string RestoreLoopSlots(string text, List<string> renderedLoopSlots, string slotToken)
    {
        if (renderedLoopSlots.Count == 0)
        {
            return text;
        }

        string result = text;
        for (int i = renderedLoopSlots.Count - 1; i >= 0; i--)
        {
            result = result.Replace($"__SWU_LOOP_{slotToken}_{i}__", renderedLoopSlots[i], StringComparison.Ordinal);
        }

        return result;
    }

    private static string ProcessIfBlocks(string template, object? model)
    {
        string current = template;

        while (IfRegex.IsMatch(current))
        {
            current = IfRegex.Replace(current, match =>
            {
                string path = match.Groups[1].Value;
                string trueBranch = match.Groups[2].Value;
                string falseBranch = match.Groups[3].Success ? match.Groups[3].Value : string.Empty;

                object? value = ResolveValue(model, path);
                return IsTruthy(value) ? trueBranch : falseBranch;
            });
        }

        return current;
    }

    private static string ReplaceVariablesSinglePass(string template, object? model)
    {
        return VariableRegex.Replace(template, match =>
        {
            bool isRaw = match.Groups[1].Success;
            string path = isRaw ? match.Groups[1].Value : match.Groups[2].Value;
            object? value = ResolveValue(model, path);
            string text = FormatValue(value);

            return isRaw ? text : WebUtility.HtmlEncode(text);
        });
    }

    private static object? ResolveValue(object? model, string path)
    {
        if (model == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (string.Equals(path, "this", StringComparison.OrdinalIgnoreCase))
        {
            return model;
        }

        string[] parts = path.Split('.');
        object? current = model;

        foreach (string part in parts)
        {
            if (current == null)
            {
                return null;
            }

            if (string.Equals(part, "this", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            current = ReadMember(current, part);
        }

        return current;
    }

    private static object? ReadMember(object target, string name)
    {
        if (target is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (string.Equals(entry.Key?.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value;
                }
            }

            return null;
        }

        Type type = target.GetType();
        PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null && prop.GetIndexParameters().Length == 0)
        {
            return prop.GetValue(target);
        }

        FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
        {
            return field.GetValue(target);
        }

        return null;
    }

    private static bool IsTruthy(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is string strValue)
        {
            return !string.IsNullOrEmpty(strValue);
        }

        if (value is int intValue)
        {
            return intValue != 0;
        }

        if (value is long longValue)
        {
            return longValue != 0;
        }

        if (value is double doubleValue)
        {
            return Math.Abs(doubleValue) > double.Epsilon;
        }

        if (value is decimal decimalValue)
        {
            return decimalValue != 0;
        }

        if (value is ICollection collection)
        {
            return collection.Count > 0;
        }

        if (value is IEnumerable enumerable)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            try
            {
                return enumerator.MoveNext();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        return true;
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is IFormattable formattable && value is not string)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        return value.ToString() ?? string.Empty;
    }
}
