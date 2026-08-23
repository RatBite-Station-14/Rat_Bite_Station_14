using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.PermaBrig;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PrisonerComponent : Component
{
    // The original mind created for this prisoner spawn.
    [DataField]
    public EntityUid? OriginalMindId;

    [DataField]
    [AutoNetworkedField]
    // The perma brig sentence in minutes, when the component was initiated
    public TimeSpan PermaBrigSentenceExpireTime = default;
}
