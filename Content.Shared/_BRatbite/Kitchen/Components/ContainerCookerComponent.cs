using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Kitchen;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Used to denote containers that can be used to cook items. <br />
/// Examples include the kitchen cooking pots.
/// <seealso cref="CookingVesselComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class ContainerCookerComponent : Component
{
    [DataField]
    public SoundSpecifier StartCookingSound = new SoundPathSpecifier("/Audio/Effects/sizzle.ogg");
}
