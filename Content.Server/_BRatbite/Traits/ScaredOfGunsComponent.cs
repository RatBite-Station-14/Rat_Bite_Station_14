namespace Content.Server._BRatbite.Traits;

[RegisterComponent]
public sealed partial class ScaredOfGunsComponent : Component
{
    [DataField]
    public float MissShotChance = 0.20f;

    [DataField]
    public float DropGunChance = 0.10f;
}
