using Content.Shared.Singularity.Components;

namespace Content.Server.Singularity.Events;

/// <summary>
/// Event raised on the target entity whenever a field generator's charge changes.
/// </summary>
[ByRefEvent]
public record struct GeneratorChargeChangedEvent(EntityUid entity, ContainmentFieldGeneratorComponent generator)
{
    /// <summary>
    /// The generator losing charge.
    /// </summary>
    public readonly EntityUid Entity = entity;

    /// <summary>
    /// The event horizon consuming the target entity.
    /// </summary>
    public readonly ContainmentFieldGeneratorComponent Generator = generator;
}
