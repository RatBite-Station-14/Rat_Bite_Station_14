using Content.Shared._BRatbite.Nutrition;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Kitchen;

/// <summary>
/// Prototype for the different methods of preparing food.
/// </summary>
[Prototype]
public sealed partial class FoodPreparationMethodPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public float CookTimeMultiplier = 1f;

    [DataField]
    public float ProductTemperature = 330f;

    [DataField]
    public List<EntProtoId> StatusEffects = [];
}
