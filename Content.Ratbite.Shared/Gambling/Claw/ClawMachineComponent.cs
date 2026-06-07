using Robust.Shared.Audio;

namespace Content.Ratbite.Shared.Gambling.Claw;

/// <summary>
/// This is used for the claw game machine.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClawMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.9f;

    [DataField]
    public SoundSpecifier PlaySound = new SoundPathSpecifier("/Audio/Machines/Keyboard/keyboard1.ogg");

    [DataField]
    public SoundSpecifier LoseSound = new SoundPathSpecifier("/Audio/Machines/buzz-two.ogg");

    [DataField]
    public SoundSpecifier WinSound = new SoundPathSpecifier("/Audio/Effects/Arcade/win.ogg");

    [DataField, AutoNetworkedField]
    public float WinChance = .10f;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;

    [DataField, AutoNetworkedField]
    public List<EntProtoId>? Rewards;

    [DataField, AutoNetworkedField]
    public List<EntProtoId>? EvilRewards;

    [DataField, AutoNetworkedField]
    public bool Emagged;
}

[Serializable, NetSerializable]
public enum ClawMachineVisuals : byte
{
    Spinning,
    NormalSprite
}
