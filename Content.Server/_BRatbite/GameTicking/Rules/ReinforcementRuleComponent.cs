using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server._BRatbite.GameTicking.Rules;

[RegisterComponent]
public sealed partial class ReinforcementRuleComponent : Component
{
    [DataField]
    public int MinReinforcementsToSpawn = 2;

    [DataField]
    public int MaxReinforcementsToSpawn = 5;

    [DataField]
    // If true, departments with fewer alive people will have a greater chance to spawn
    public bool PrioritizeUnderstaffedDepartments = true;

    [DataField(customTypeSerializer: typeof(DictionarySerializer<ProtoId<DepartmentPrototype>, ReinforcementDefinition>))]
    public Dictionary<ProtoId<DepartmentPrototype>, ReinforcementDefinition> ReinforcementsPrototypes = new();
}

[DataDefinition]
public sealed partial class ReinforcementDefinition
{
    [DataField]
    public List<EntProtoId> Prototypes = new();

    [DataField]
    public float WeightModifier = 1f;

    public ReinforcementDefinition(List<EntProtoId> prototypes, float weightModifier = 1f)
    {
        WeightModifier = weightModifier;
        Prototypes = prototypes;
    }
}
