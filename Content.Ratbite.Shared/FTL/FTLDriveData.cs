namespace Content.Ratbite.Shared.FTL;

/// <summary>
/// Contains data for the FTL drive.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public partial record struct FTLDriveData
{
    public FTLDriveData(float range, bool ftlToSameMap)
    {
        Range = range;
        FTLToSameMap = ftlToSameMap;
    }

    [DataField]
    public float Range;

    [DataField("ftlToSameMap")]
    public bool FTLToSameMap;

    [DataField]
    public float? StartupTime;

    [DataField]
    public float? KnockdownTime;

    [DataField]
    public float? TravelTime;

    [DataField]
    public float? ArrivalTime;

    [DataField]
    public float? CooldownTime;
}
