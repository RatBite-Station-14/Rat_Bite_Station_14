using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Weapons.Ranged;

[RegisterComponent]
public sealed partial class CombatUntrainedComponent : Component
{
    [DataField]
    public float RecoilDebuff = 2f;

    [DataField]
    public float FlatRecoilDebuff = 5f;

    [DataField]
    public float MissShotChance = 0.05f;

    [DataField]
    public float DropGunChance = 0.01f;
}
