using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Silicon;

[RegisterComponent]
public sealed partial class TemperatureBatteryDrainComponent : Component
{
    [DataField]
    public float OptimalTemperature = 293f;

    [ViewVariables]
    public float OriginalDrain;
}
