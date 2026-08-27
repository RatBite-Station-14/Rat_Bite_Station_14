using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Movement;

[Serializable, NetSerializable]
public sealed class LeapWallImpactEvent : EntityEventArgs
{
    public NetEntity Entity;
    public TimeSpan KnockdownDuration;

    public LeapWallImpactEvent(NetEntity entity, TimeSpan knockdownDuration)
    {
        Entity = entity;
        KnockdownDuration = knockdownDuration;
    }
}
