using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using ChangeElementLevel.Localization;

namespace ChangeElementLevel;

/// <summary>
/// Regista um separador na ribbon com o comando principal.
/// Textos e tema seguem o idioma e o tema do Revit (sem definições no add-in).
/// </summary>
public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        LanguageType language = application.ControlledApplication.Language;
        PluginStrings.Load(language);

        string tabName = PluginStrings.T("ribbon.tab");
        try
        {
            application.CreateRibbonTab(tabName);
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
        {
            // O separador já existe (recarregamento do add-in).
        }

        string panelName = PluginStrings.T("ribbon.panel");
        RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);
        string assemblyPath = typeof(App).Assembly.Location;

        PushButtonData buttonData = new(
            "ChangeLevelPreservePosition",
            PluginStrings.T("ribbon.buttonShort"),
            assemblyPath,
            $"{typeof(Commands.ChangeLevelPreservePositionCommand).FullName}")
        {
            ToolTip = PluginStrings.T("ribbon.tooltip"),
            LongDescription = PluginStrings.T("ribbon.longDescription")
        };

        panel.AddItem(buttonData);
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}
