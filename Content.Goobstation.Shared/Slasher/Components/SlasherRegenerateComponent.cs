using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Basically just injects whatever chemical you want and breaks cuffs.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherRegenerateComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherRegenerate";

    /// <summary>
    /// The reagent to inject
    /// </summary>
    [DataField("reagent")]
    public ProtoId<ReagentPrototype> Reagent = "slasherium";

    /// <summary>
    /// How much reagent to inject
    /// </summary>
    [DataField("reagentAmount")]
    public float ReagentAmount = 10f;

    // <Ratbite> - Regenerate can now store more souls

    /// <summary>
    /// How many souls the slasher has for the Regenerate ability
    /// Acts as ammo for the regenerate ability.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int SoulsAvailable = 1; // Start with one soul available   Ratbite - Now integer, see summary above

    /// <summary>
    /// The max amount of souls that Regenerate can store
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxSouls = 3; // Max 3 souls can be stored

    // </Ratbite>

    /// <summary>
    /// The sound that plays when regenerating
    /// </summary>
    [DataField]
    public SoundSpecifier? RegenerateSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/SlasherRegenerate.ogg");

    /// <summary>
    /// The effect entity that is spawned when regenerating (includes light, sprite, and auto-despawn)
    /// </summary>
    [DataField]
    public EntProtoId RegenerateEffect = "SlasherRegenerateEffect";
}


