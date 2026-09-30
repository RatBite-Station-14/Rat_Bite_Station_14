using Content.Shared._BRatbite.Kitchen.Systems;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Used to denote Container Cookers that are actively cooking.
/// <seealso cref="ContainerCookerComponent"/>
/// <seealso cref="SharedContainerCookerSystem"/>
/// </summary>
[RegisterComponent]
public sealed partial class ActiveContainerCookerComponent : Component
{
    [DataField]
    public EntityUid HeatSource = EntityUid.Invalid;
}
