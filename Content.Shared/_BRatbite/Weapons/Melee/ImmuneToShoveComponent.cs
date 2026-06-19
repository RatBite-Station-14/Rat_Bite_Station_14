namespace Content.Shared._BRatbite.Weapons.Melee;

[RegisterComponent]
public sealed partial class ImmuneToShoveComponent : Component
{
    [DataField]
    public bool ImmuneToPush = true;

    [DataField]
    public bool ImmuneToThrow = true;
}

[RegisterComponent]
public sealed partial class ImmuneToShoveOnDeathComponent : Component;
