using Robust.Shared.Audio;

namespace Content.Ratbite.Shared.Bank;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BankTerminalComponent : Component
{
    [DataField]
    public SoundSpecifier? SoundOnTransfer = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg", AudioParams.Default.WithVariation(0.05f));

    [DataField]
    public SoundSpecifier? SoundOnCreateAccount = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg", AudioParams.Default.WithVariation(0.05f));

    [DataField, AutoNetworkedField]
    public string LinkedAccount = "";

    [DataField, AutoNetworkedField]
    public string LinkedPassword = "";

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedBank = null;
}
