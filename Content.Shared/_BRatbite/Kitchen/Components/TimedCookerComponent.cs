using Content.Shared._BRatbite.Kitchen.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Cooking machines w/ time presets.<br />
/// Examples include the microwave, oven, and preservatronic.
/// <seealso cref="CookingVesselComponent"/>
/// <seealso cref="SharedTimedCookerSystem"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TimedCookerComponent : Component
{
    /// <summary>
    /// A list of durations used in construction of the UI's buttons. <br />
    /// Also used on the server to validate times sent by the client.
    /// </summary>
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
