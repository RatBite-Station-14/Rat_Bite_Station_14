using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition;

[Prototype]
public sealed partial class FoodFreshnessPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField]
    // What effects does the food give when eaten with this freshness level
    public List<EntProtoId> Effects = new();

    [DataField]
    public string? ExamineText;
}

[Prototype]
// Describes how a food ages. For example, cheese gets better with
// age, while other food might not
public sealed partial class FoodDecayPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField]
    // Starting freshness -> what it decays to
    public Dictionary<ProtoId<FoodFreshnessPrototype>, (TimeSpan, ProtoId<FoodFreshnessPrototype>)> DecayTimes = new () { };
}
