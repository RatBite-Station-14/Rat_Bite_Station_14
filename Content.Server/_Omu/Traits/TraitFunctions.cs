using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared._Omu.Traits;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;

namespace Content.Server._Omu.Traits;

/// <summary>
///     Used for traits that remove a component upon a player spawning in.
/// </summary>
/// <remarks>
///     taken from EE. 
/// </remarks>
public sealed partial class TraitRemoveComponent : TraitFunction
{
    [DataField(required: true), AlwaysPushInheritance]
    public ComponentRegistry ComponentsToRemove { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var (name, _) in ComponentsToRemove)
            entityManager.RemoveComponentDeferred(uid, factory.GetComponent(name).GetType());
    }
}

/// <remarks>
///     taken from EE. 
/// </remarks>
public sealed partial class TraitModifyDensity : TraitFunction
{
    [DataField(required: true), AlwaysPushInheritance]
    public float DensityModifier;

    [DataField, AlwaysPushInheritance]
    public bool? Multiply;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var physicsSystem = entityManager.System<SharedPhysicsSystem>();
        if (!entityManager.TryGetComponent<FixturesComponent>(uid, out var fixturesComponent)
            || fixturesComponent.Fixtures.Count is 0)
            return;

        var fixture = fixturesComponent.Fixtures["fix1"]; // As of writing, fix1 is the fixture used for practically everything.
        var newDensity = (Multiply ?? false) ? fixture.Density * DensityModifier : fixture.Density + DensityModifier;
        physicsSystem.SetDensity(uid, "fix1", fixture, newDensity);
        // SetDensity handles the Dirty.
    }
}

/// <summary>
///     Used for traits that modify unarmed damage on MeleeWeaponComponent.
/// </summary>
/// <remarks>
///     taken from EE. 
/// </remarks>
public sealed partial class TraitModifyUnarmed : TraitFunction
{
    // <summary>
    //     The sound played on hitting targets.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public SoundSpecifier? HitSound;

    // <summary>
    //     The animation to play on hit, for both light and power attacks.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public EntProtoId? Animation;

    // <summary>
    //     Whether to set the power attack animation to be the same as the light attack.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public bool? HeavyAnimationFromLight;

    // <summary>
    //     The damage values of unarmed damage.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public DamageSpecifier? Damage;

    // <summary>
    //     Additional damage added to the existing damage.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public DamageSpecifier? FlatDamageIncrease;

    // <summary>
    //     What to multiply the melee weapon range by.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public float? RangeModifier;

    // <summary>
    //     What to multiply the attack rate by.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public float? AttackRateModifier;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<MeleeWeaponComponent>(uid, out var melee))
            return;

        if (HitSound != null)
            melee.HitSound = HitSound;

        if (Animation != null)
            melee.Animation = Animation.Value;

        if (HeavyAnimationFromLight ?? true)
            melee.WideAnimation = melee.Animation;

        if (Damage != null)
            melee.Damage = Damage;

        if (FlatDamageIncrease != null)
            melee.Damage += FlatDamageIncrease;

        if (RangeModifier != null)
            melee.Range *= RangeModifier.Value;

        if (AttackRateModifier != null)
            melee.AttackRate *= AttackRateModifier.Value;

        entityManager.Dirty(uid, melee);
    }
}
