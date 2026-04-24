using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ChangeElementLevel.Localization;
using ChangeElementLevel.Services;
using ChangeElementLevel.Views;

namespace ChangeElementLevel.Commands;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class ChangeLevelPreservePositionCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiApp = commandData.Application;
        PluginStrings.Load(uiApp.Application.Language);

        UIDocument? uidoc = uiApp.ActiveUIDocument;
        if (uidoc is null)
        {
            TaskDialog.Show(
                PluginStrings.T("cmd.noSelectionTitle"),
                PluginStrings.T("cmd.noActiveDocument"));
            return Result.Cancelled;
        }

        Document doc = uidoc.Document;
        ICollection<ElementId> selection = uidoc.Selection.GetElementIds();

        if (selection.Count == 0)
        {
            TaskDialog.Show(
                PluginStrings.T("cmd.noSelectionTitle"),
                PluginStrings.T("cmd.noSelectionBody"));
            return Result.Cancelled;
        }

        ChangeLevelDialog dialog = new(doc, uiApp);
        dialog.SetRevitOwner(uiApp);

        if (dialog.ShowDialog() != true || dialog.SelectedLevel is null)
            return Result.Cancelled;

        Level targetLevel = dialog.SelectedLevel;
        var report = new StringBuilder();
        int ok = 0, skipped = 0;

        using Transaction t = new(doc, PluginStrings.T("cmd.transactionName"));
        t.Start();

        foreach (ElementId id in selection)
        {
            Element? el = doc.GetElement(id);
            if (el is null)
                continue;

            if (LevelPreserveService.TryReassignLevel(doc, el, targetLevel, out string? failReason))
                ok++;
            else
            {
                skipped++;
                report.AppendLine($"• {ElementDescription(el)}: {failReason}");
            }
        }

        t.Commit();

        string summary = PluginStrings.T("cmd.summaryUpdated", ok);
        if (skipped > 0)
            summary += PluginStrings.T("cmd.summarySkippedPart", skipped);

        if (report.Length > 0)
            summary += PluginStrings.T("cmd.ignoredHeader") + report;

        TaskDialog.Show(PluginStrings.T("cmd.taskTitle"), summary);
        return Result.Succeeded;
    }

    private static string ElementDescription(Element el)
    {
        string name = el.Name;
        if (string.IsNullOrWhiteSpace(name))
            name = el.Category?.Name ?? PluginStrings.T("generic.element");
        return $"{name} (id {el.Id.Value})";
    }
}
