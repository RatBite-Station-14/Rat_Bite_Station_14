using Content.Shared.Roles;
using Content.Shared.Contraband;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

/// <summary>
/// Makes the entity be considered contraband
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangeContrabandStatusEffectComponent : Component
{
    /// <summary>
    /// The degree of contraband severity the entity will get.
    /// </summary>
    [DataField]
    public ProtoId<ContrabandSeverityPrototype> Severity = "Restricted";

    /// <summary>
    /// Which departments will the entity be restricted to?
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> AllowedDepartments = new();

    /// <summary>
    /// Which jobs is will the entity be restricted to?
    /// </summary>
    [DataField]
    public HashSet<ProtoId<JobPrototype>> AllowedJobs = new();

    /// <summary>
    /// Tells if the entity had the contraband component before the effect was applied.
    /// </summary>
    [ViewVariables]
    public bool EntityHadContrabandComponent = false;

    /// <summary>
    /// If the entity already had contraband component, the severity is stored here until it can
    /// be returned when the effect is over
    /// </summary>
    [ViewVariables]
    public ProtoId<ContrabandSeverityPrototype>? OldSeverity;

    /// <summary>
    /// If the entity already had contraband component, the allowed departments is stored here until it can
    /// be returned when the effect is over
    /// </summary>
    [ViewVariables]
    public HashSet<ProtoId<DepartmentPrototype>>? OldAllowedDepartments;

    /// <summary>
    /// If the entity already had contraband component, the allowed jobs is stored here until it can
    /// be returned when the effect is over
    /// </summary>
    [ViewVariables]
    public HashSet<ProtoId<JobPrototype>>? OldAllowedJobs;
}
