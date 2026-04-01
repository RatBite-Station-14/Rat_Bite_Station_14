using Content.Shared.Access.Systems;
using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Access.Components;

/// <summary>
/// This is used for an ID that expires and replaces its access after a certain period has passed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ExpireIdCardComponent : Component
{
    /// <summary>
    /// Whether this ID has expired yet and had its accesses replaced.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Expired;

    /// <summary>
    /// Whether this card will expire at all.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Permanent;

    /// <summary>
    /// The time at which this card will expire and the access will be removed.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan ExpireTime = TimeSpan.Zero;

    /// <summary>
    /// Access the replaces current access once this card expires.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> ExpiredAccess = new();

    /// <summary>
    /// Line spoken by the card when it expires.
    /// </summary>
    [DataField]
    public LocId? ExpireMessage;

    /// <summary>
    /// The channel to transmit the expire message in. Null if it shouldn't broadcast!
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype>? ExpireChannel = null;
}
