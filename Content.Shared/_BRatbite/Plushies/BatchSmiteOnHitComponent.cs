using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Plushies;

[RegisterComponent]
public sealed partial class BatchSmiteOnHitComponent : Component
{
    // 0.001% trigger chance per successful melee hit.
    [DataField]
    public float TriggerChance = 0.00001f;

    // If the rare trigger happens, attacker gets explode-smited 80% of the time.
    [DataField]
    public float SelfExplodeChance = 0.8f;
}
