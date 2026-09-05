namespace Content.Server._BRatbite.Traits;

[RegisterComponent]
public sealed partial class TriggerComponent : Component
{
    [DataField]
    public float TotalIntensity = 50;
    [DataField]
    public float Slope = 3;
    [DataField]
    public float MaxTileIntensity = 1;
    [DataField]
    public bool Examined = false;
}

