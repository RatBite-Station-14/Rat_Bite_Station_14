using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.LimitWeapon;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LimitWeaponComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public string MeleeFail = "weapon-general-fail";

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public string GunFail = "weapon-general-fail";

    public TimeSpan LastPopup;

    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);
}
