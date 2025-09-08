using Robust.Shared.Serialization.Manager;

namespace Content.Shared._Omu.Traits;

/// <summary>
///     This serves as a hook for trait functions to modify a player character upon spawning in.
/// </summary>
/// <remarks>
///     Think of this in the same way that you would a JobRequirement.
///     Inheritors of this class can be found in Content.Server, this is only in Content.Shared so that the upstream TraitPrototype can find the type of those inheritors. Polymorphism does the rest for us.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
public abstract partial class TraitFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
