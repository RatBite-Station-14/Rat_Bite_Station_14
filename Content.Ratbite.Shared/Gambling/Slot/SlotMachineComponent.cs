using Robust.Shared.Audio;

namespace Content.Ratbite.Shared.Gambling.Slot;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlotMachineComponent : Component
{
    [DataField]
    public SoundSpecifier SpinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_spin.ogg");

    [DataField]
    public SoundSpecifier LoseSound = new SoundPathSpecifier("/Audio/Machines/buzz-two.ogg");

    [DataField]
    public SoundSpecifier SmallWinSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    [DataField]
    public SoundSpecifier MediumWinSound = new SoundPathSpecifier("/Audio/Effects/Arcade/win.ogg");

    [DataField]
    public SoundSpecifier BigWinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_bigwin.ogg");

    [DataField]
    public SoundSpecifier JackPotWinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_jackpotwin.ogg");

    [DataField]
    public SoundSpecifier GodPotWinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_godpot.ogg");

    [DataField, AutoNetworkedField]
    public float SmallWinChance = .20f;

    [DataField, AutoNetworkedField]
    public float MediumWinChance = .10f;

    [DataField, AutoNetworkedField]
    public float BigWinChance = .05f;

    [DataField, AutoNetworkedField]
    public float JackPotWinChance = .01f;

    [DataField, AutoNetworkedField]
    public float GodPotWinChance = .0001f;

    [DataField, AutoNetworkedField]
    public EntProtoId GodPotPrize = "WeaponShotgunHeavy";

    [DataField, AutoNetworkedField]
    public bool Emagged;

    [DataField, AutoNetworkedField]
    public int SpinCost = 250;

    [DataField, AutoNetworkedField]
    public int SmallPrizeAmount = 250;

    [DataField, AutoNetworkedField]
    public int MediumPrizeAmount = 500;

    [DataField, AutoNetworkedField]
    public int BigPrizeAmount = 5000;

    [DataField, AutoNetworkedField]
    public int JackPotPrizeAmount = 20000;

    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.8f;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;
}

[Serializable, NetSerializable]
public enum SlotMachineVisuals : byte
{
    Spinning
}
