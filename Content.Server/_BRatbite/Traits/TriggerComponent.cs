namespace Content.Server._BRatbite.Traits;

[RegisterComponent]
public sealed partial class TriggerComponent : Component
{
    [DataField]
    public float TotalIntensity = 100;
    [DataField]
    public float Slope = 3;
    [DataField]
    public float MaxTileIntensity = 4;
    [DataField]
    public bool Examined = false;
}

