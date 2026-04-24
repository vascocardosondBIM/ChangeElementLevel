using Autodesk.Revit.DB;

namespace ChangeElementLevel.Views;

public sealed class LevelListRow
{
    public Level Level { get; }
    public string Title => Level.Name;
    public string ElevationCaption { get; }

    public LevelListRow(Level level, string elevationCaption)
    {
        Level = level;
        ElevationCaption = elevationCaption;
    }

    public static string FormatElevation(Document doc, Level level)
    {
        try
        {
            return UnitFormatUtils.Format(doc.GetUnits(), SpecTypeId.Length, level.Elevation, false);
        }
        catch
        {
            return level.Elevation.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
