using Autodesk.Revit.ApplicationServices;

namespace ChangeElementLevel.Localization;

/// <summary>
/// Mapeia o idioma instalado do Revit para o ficheiro JSON em <c>i18n</c>.
/// </summary>
public static class RevitLanguageMapper
{
    public static string GetCultureFileName(LanguageType language) =>
        language switch
        {
            LanguageType.French => "fr-FR",
            LanguageType.German => "de-DE",
            LanguageType.Spanish => "es-ES",
            LanguageType.Brazilian_Portuguese => "pt-BR",
            LanguageType.English_GB => "en-US",
            LanguageType.English_USA => "en-US",
            _ => "en-US"
        };
}
