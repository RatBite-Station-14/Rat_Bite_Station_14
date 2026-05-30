namespace Content.Shared._BRatbite.Weapons.Melee;

[RegisterComponent]
public sealed partial class CaptainSaberWeaknessComponent : Component
{
    [DataField]
    public float DamageMultiplier = 1.2f;
}
