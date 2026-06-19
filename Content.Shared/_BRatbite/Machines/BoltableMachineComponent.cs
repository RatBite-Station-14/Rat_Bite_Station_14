using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared._BRatbite.Machines;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BoltableMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Bolted = true;

    [DataField]
    public SoundSpecifier BoltSound = new SoundPathSpecifier("/Audio/Machines/boltsdown.ogg");

    [DataField]
    public SoundSpecifier UnboltSound = new SoundPathSpecifier("/Audio/Machines/boltsup.ogg");

    [DataField]
    public string? AnchorFailedMessage = "machine-popup-cant-anchor";
}
