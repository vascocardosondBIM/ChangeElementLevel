using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Autodesk.Revit.ApplicationServices;

namespace ChangeElementLevel.Localization;

/// <summary>
/// Textos: primeiro ficheiros em <c>i18n</c> junto à DLL (opcional), senão recursos embutidos na assembly.
/// </summary>
public static class PluginStrings
{
    private static Dictionary<string, string> _map = new(StringComparer.Ordinal);
    private static readonly object Sync = new();

    public static void Load(LanguageType language)
    {
        string culture = RevitLanguageMapper.GetCultureFileName(language);
        Assembly asm = Assembly.GetExecutingAssembly();

        string? json =
            TryReadFromDisk(asm, culture)
            ?? TryReadEmbedded(asm, culture)
            ?? TryReadEmbedded(asm, "en-US");

        if (string.IsNullOrWhiteSpace(json))
            json = "{}";

        Dictionary<string, string>? parsed =
            JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        lock (Sync)
            _map = parsed ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static string? TryReadFromDisk(Assembly asm, string culture)
    {
        string? dir = Path.GetDirectoryName(asm.Location);
        if (string.IsNullOrEmpty(dir))
            return null;

        string culturePath = Path.Combine(dir, "i18n", $"{culture}.json");
        if (File.Exists(culturePath))
            return File.ReadAllText(culturePath, Encoding.UTF8);

        string fallback = Path.Combine(dir, "i18n", "en-US.json");
        if (File.Exists(fallback))
            return File.ReadAllText(fallback, Encoding.UTF8);

        return null;
    }

    private static string? TryReadEmbedded(Assembly asm, string culture)
    {
        string? name = FindEmbeddedResourceName(asm, culture);
        if (name is null)
            return null;

        using Stream? stream = asm.GetManifestResourceStream(name);
        if (stream is null)
            return null;

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static string? FindEmbeddedResourceName(Assembly asm, string culture)
    {
        string suffix = $".{culture}.json";
        foreach (string n in asm.GetManifestResourceNames())
        {
            if (n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return n;
        }

        return null;
    }

    public static string T(string key)
    {
        lock (Sync)
            return _map.TryGetValue(key, out string? v) ? v : key;
    }

    public static string T(string key, object arg0) =>
        string.Format(T(key), arg0);

    public static string T(string key, object arg0, object arg1) =>
        string.Format(T(key), arg0, arg1);
}
