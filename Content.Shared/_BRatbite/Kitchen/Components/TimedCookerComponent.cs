using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Cooking machines w/ time presets. Such as the microwave, oven, or preservatronic.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TimedCookerComponent : Component
{
    [DataField, AutoNetworkedField]
    public uint[] CookTimePresets = [5, 10, 15, 20, 25, 30];

    #region audio

    [DataField, AutoNetworkedField]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier FoodDoneSound = new SoundPathSpecifier("/Audio/Machines/microwave_done_beep.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier StartCookingSound = new SoundPathSpecifier("/Audio/Machines/microwave_start_beep.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? LoopingSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? OnUIOpenSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? OnUICloseSound;

    public EntityUid? PlayingStream;

    #endregion
}
