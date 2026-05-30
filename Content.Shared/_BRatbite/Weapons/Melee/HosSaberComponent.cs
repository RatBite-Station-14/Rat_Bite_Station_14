namespace Content.Shared._BRatbite.Weapons.Melee;

// Hos saber has buff against perma prisoners
[RegisterComponent]
public sealed partial class HosSaberComponent : Component
{
    [DataField]
    public float DamageMultiplierPerMinute = 0.0005f;
}
