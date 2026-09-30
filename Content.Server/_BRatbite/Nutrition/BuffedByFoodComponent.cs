using Content.Trauma.Shared.Timing;

namespace Content.Server._BRatbite.Nutrition;

[RegisterComponent]
/// <summary>
///     Marks entities that can be buffed by food
/// </summary>
public sealed partial class BuffedByFoodComponent : Component
{
    [ViewVariables]
    public TimedRingBuffer<Buff> ActivatedBuffs = default!;
}

public record struct Buff(List<EntityUid> Ents);

