using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Access;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmergencyAccessComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public bool EmergencyAccess = false;

    /// <summary>
    /// Sound to play when the airlock emergency access is turned on.
    /// </summary>
    [DataField]
    public SoundSpecifier EmergencyOnSound = new SoundPathSpecifier("/Audio/Machines/airlock_emergencyon.ogg");

    /// <summary>
    /// Sound to play when the airlock emergency access is turned off.
    /// </summary>
    [DataField]
    public SoundSpecifier EmergencyOffSound = new SoundPathSpecifier("/Audio/Machines/airlock_emergencyoff.ogg");
}

[ByRefEvent]
public record struct EmergencyAccessChangedEvent(bool IsEnabled);
