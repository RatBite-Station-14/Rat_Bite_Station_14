using Content.Shared._BRatbite.Kitchen.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Used by cooking systems to prevent reactions of their ingredients while they are being used in a recipe, and likely heated or transformed.
///
/// <seealso cref="SharedCookingVesselSystem"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BeingUsedInRecipeComponent : Component
{
    /// <summary>
    /// The vessel that owns this ingredient.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid OwnedBy = EntityUid.Invalid;
}
