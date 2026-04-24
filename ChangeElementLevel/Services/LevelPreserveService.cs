using Autodesk.Revit.DB;
using ChangeElementLevel.Localization;

namespace ChangeElementLevel.Services;

/// <summary>
/// Recalcula nível + offset para manter a cota Z do elemento (coordenadas internas do modelo).
/// </summary>
public static class LevelPreserveService
{
    private static readonly BuiltInParameter[] OffsetCandidates =
    [
        BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM,
        BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM,
        BuiltInParameter.INSTANCE_ELEVATION_PARAM,
        BuiltInParameter.Z_OFFSET_VALUE
    ];

    public static bool TryReassignLevel(Document doc, Element element, Level newLevel, out string? failReason)
    {
        failReason = null;

        if (element.Pinned)
        {
            failReason = PluginStrings.T("errors.pinned");
            return false;
        }

        return element switch
        {
            FamilyInstance fi => TryFamilyInstance(doc, fi, newLevel, out failReason),
            Wall wall => TryWall(doc, wall, newLevel, out failReason),
            Floor floor => TryFloor(doc, floor, newLevel, out failReason),
            Ceiling ceiling => TryCeiling(doc, ceiling, newLevel, out failReason),
            _ => Unsupported(element, out failReason)
        };
    }

    private static bool Unsupported(Element element, out string? failReason)
    {
        failReason = PluginStrings.T("errors.unsupportedType", element.GetType().Name);
        return false;
    }

    private static bool TryFamilyInstance(Document doc, FamilyInstance fi, Level newLevel, out string? failReason)
    {
        failReason = null;

        if (fi.Host is not null && fi.Host is not Level)
        {
            failReason = PluginStrings.T("errors.hostedNonLevel");
            return false;
        }

        if (!TryGetReferenceZ(fi, out double refZ))
        {
            failReason = PluginStrings.T("errors.noZ");
            return false;
        }

        ElementId currentLevelId = fi.LevelId;
        if (currentLevelId == ElementId.InvalidElementId)
        {
            failReason = PluginStrings.T("errors.noLevelRef");
            return false;
        }

        if (currentLevelId == newLevel.Id)
        {
            failReason = PluginStrings.T("errors.alreadyLevel");
            return false;
        }

        if (doc.GetElement(currentLevelId) is not Level)
        {
            failReason = PluginStrings.T("errors.badCurrentLevel");
            return false;
        }

        double newElev = newLevel.Elevation;
        double newOffset = refZ - newElev;

        if (!TrySetFamilyInstanceLevel(fi, newLevel.Id, out failReason))
            return false;

        if (!TrySetVerticalOffset(fi, newOffset, out failReason))
        {
            TrySetFamilyInstanceLevel(fi, currentLevelId, out _);
            return false;
        }

        return true;
    }

    private static bool TryWall(Document doc, Wall wall, Level newLevel, out string? failReason)
    {
        failReason = null;

        Parameter? baseConstraint = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
        Parameter? baseOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);

        if (baseConstraint is null || baseOffset is null || baseConstraint.StorageType != StorageType.ElementId)
        {
            failReason = PluginStrings.T("errors.wallBaseUnavailable");
            return false;
        }

        ElementId baseLevelId = baseConstraint.AsElementId();
        if (baseLevelId == ElementId.InvalidElementId || doc.GetElement(baseLevelId) is not Level oldBase)
        {
            failReason = PluginStrings.T("errors.wallBaseInvalid");
            return false;
        }

        if (baseLevelId == newLevel.Id)
        {
            failReason = PluginStrings.T("errors.wallBaseSameLevel");
            return false;
        }

        double baseZ = oldBase.Elevation + baseOffset.AsDouble();
        double newBaseOffset = baseZ - newLevel.Elevation;

        if (baseConstraint.IsReadOnly || baseOffset.IsReadOnly)
        {
            failReason = PluginStrings.T("errors.wallReadonly");
            return false;
        }

        baseConstraint.Set(newLevel.Id);
        baseOffset.Set(newBaseOffset);
        return true;
    }

    private static bool TryFloor(Document doc, Floor floor, Level newLevel, out string? failReason)
    {
        failReason = null;

        Parameter? levelParam = floor.get_Parameter(BuiltInParameter.LEVEL_PARAM);
        Parameter? heightParam = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

        if (levelParam is null || heightParam is null || levelParam.StorageType != StorageType.ElementId)
        {
            failReason = PluginStrings.T("errors.floorParamsMissing");
            return false;
        }

        ElementId levelId = levelParam.AsElementId();
        if (levelId == ElementId.InvalidElementId || doc.GetElement(levelId) is not Level oldLevel)
        {
            failReason = PluginStrings.T("errors.floorLevelInvalid");
            return false;
        }

        if (levelId == newLevel.Id)
        {
            failReason = PluginStrings.T("errors.floorSameLevel");
            return false;
        }

        double refZ = oldLevel.Elevation + heightParam.AsDouble();
        double newHeight = refZ - newLevel.Elevation;

        if (levelParam.IsReadOnly || heightParam.IsReadOnly)
        {
            failReason = PluginStrings.T("errors.floorReadonly");
            return false;
        }

        levelParam.Set(newLevel.Id);
        heightParam.Set(newHeight);
        return true;
    }

    private static bool TryCeiling(Document doc, Ceiling ceiling, Level newLevel, out string? failReason)
    {
        failReason = null;

        Parameter? levelParam = ceiling.get_Parameter(BuiltInParameter.LEVEL_PARAM);
        Parameter? heightParam = ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM);

        if (levelParam is null || heightParam is null || levelParam.StorageType != StorageType.ElementId)
        {
            failReason = PluginStrings.T("errors.ceilingParamsMissing");
            return false;
        }

        ElementId levelId = levelParam.AsElementId();
        if (levelId == ElementId.InvalidElementId || doc.GetElement(levelId) is not Level oldLevel)
        {
            failReason = PluginStrings.T("errors.ceilingLevelInvalid");
            return false;
        }

        if (levelId == newLevel.Id)
        {
            failReason = PluginStrings.T("errors.ceilingSameLevel");
            return false;
        }

        double refZ = oldLevel.Elevation + heightParam.AsDouble();
        double newHeight = refZ - newLevel.Elevation;

        if (levelParam.IsReadOnly || heightParam.IsReadOnly)
        {
            failReason = PluginStrings.T("errors.ceilingReadonly");
            return false;
        }

        levelParam.Set(newLevel.Id);
        heightParam.Set(newHeight);
        return true;
    }

    private static bool TryGetReferenceZ(FamilyInstance fi, out double z)
    {
        z = 0;
        switch (fi.Location)
        {
            case LocationPoint lp:
                z = lp.Point.Z;
                return true;
            case LocationCurve lc:
                XYZ p0 = lc.Curve.GetEndPoint(0);
                XYZ p1 = lc.Curve.GetEndPoint(1);
                z = 0.5 * (p0.Z + p1.Z);
                return true;
            default:
                return false;
        }
    }

    private static bool TrySetFamilyInstanceLevel(FamilyInstance fi, ElementId newLevelId, out string? failReason)
    {
        failReason = null;

        Parameter? p = fi.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                       ?? fi.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);

        if (p is not null && p.StorageType == StorageType.ElementId && !p.IsReadOnly)
        {
            try
            {
                p.Set(newLevelId);
                return true;
            }
            catch (Exception ex)
            {
                failReason = PluginStrings.T("errors.levelParameterException", ex.Message);
                return false;
            }
        }

        failReason = PluginStrings.T("errors.levelParamReadonly");
        return false;
    }

    private static bool TrySetVerticalOffset(FamilyInstance fi, double offsetFeet, out string? failReason)
    {
        failReason = null;

        foreach (BuiltInParameter bip in OffsetCandidates)
        {
            Parameter? p = fi.get_Parameter(bip);
            if (p is null || p.IsReadOnly || p.StorageType != StorageType.Double)
                continue;

            try
            {
                p.Set(offsetFeet);
                return true;
            }
            catch
            {
                // Tentar o próximo candidato.
            }
        }

        failReason = PluginStrings.T("errors.offsetMissing");
        return false;
    }
}
