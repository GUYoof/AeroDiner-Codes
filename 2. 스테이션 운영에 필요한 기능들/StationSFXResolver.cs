public static class StationSFXResolver
{
    public static SFXType GetSFXFromStationData(StationData data)
    {
        SFXType resolved = SFXType.BlankClick;

        switch (data.workType)
        {
            case WorkType.Automatic:
                resolved = GetAutomaticSFX(data.stationType);
                break;
            case WorkType.Passive:
                resolved = GetPassiveSFX(data.stationType);
                break;
            default:
                resolved = SFXType.BlankClick;
                break;
        }

        return resolved;
    }

    private static SFXType GetAutomaticSFX(StationType type)
    {
        switch (type)
        {
            case StationType.Oven: return SFXType.Oven;
            case StationType.Pot: return SFXType.Pot;
            case StationType.Grater: return SFXType.AutoGrater;
            case StationType.CuttingBoard: return SFXType.AutoCutter;
            case StationType.Grinding: return SFXType.Blender;
            case StationType.FryingPan: return SFXType.RollPan;
            default: return SFXType.BlankClick;
        }
    }

    private static SFXType GetPassiveSFX(StationType type)
    {
        switch (type)
        {
            case StationType.Grater: return SFXType.Grater;
            case StationType.CuttingBoard: return SFXType.CuttingBoard;
            case StationType.Grinding: return SFXType.Blender;
            case StationType.FryingPan: return SFXType.FryingPan;
            default: return SFXType.BlankClick;
        }
    }
}
