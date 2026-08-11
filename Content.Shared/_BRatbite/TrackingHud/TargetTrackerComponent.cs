using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.TrackingHud;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedTrackingTargetSystem))]
public sealed partial class TargetTrackerComponent : Component
{
    // Don't spam other clients with this
    public override bool SendOnlyToOwner => true;

    [DataField, AutoNetworkedField]
    public Dictionary<string, TrackingTarget> Targets = new();

    [DataField]
    public ListeningChannels Channels = ListeningChannels.SECURITY;
}

[Serializable, NetSerializable, Flags]
public enum ListeningChannels : byte
{
    NONE = 0,
    SECURITY = 1 << 0,
    SILICON = 1 << 1,
}

[Serializable, NetSerializable, DataDefinition]
public partial struct TrackingTarget
{
    [DataField]
    public Vector2 TargetLocation;

    [DataField]
    public Color PinColor = new Color(255, 0, 0, 180);

    [DataField]
    public MapId MapId;

    [DataField]
    public SpriteSpecifier Sprite = new SpriteSpecifier.Rsi(new("/Textures/_BRatBites/Interface/Misc/exclamation-mark.rsi"), "exclamation-mark");

    [DataField]
    public ListeningChannels Channels = ListeningChannels.SECURITY;
}
