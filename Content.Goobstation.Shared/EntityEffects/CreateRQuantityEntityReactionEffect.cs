// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reaction;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.EntityEffects;

// i dont even know if this works. if you're reading this, it likely doesn't. Change the Comp.
// Ratbite: Changed to transform component so it doesn't break when it becomes unreactive
public sealed partial class CreateRQuantityEntityReactionEffectSystem : EntityEffectSystem<TransformComponent, CreateRQuantityEntityReactionEffect>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<CreateRQuantityEntityReactionEffect> args)
    {
        var quantity = _random.Next(1, args.Effect.MaxEntities + 1);

        for (var i = 0; i < quantity; i++)
        {
            SpawnNextToOrDrop(args.Effect.Entity, entity); // Ratbite
        }
    }
}

[DataDefinition]
public sealed partial class CreateRQuantityEntityReactionEffect : EntityEffectBase<CreateRQuantityEntityReactionEffect>
{
    /// <summary>
    ///     What entity to create.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Entity = default!;

    /// <summary>
    ///     What is our maximum allowed entities to be spawned?
    /// </summary>
    [DataField]
    public int MaxEntities = 1;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-create-entity-reaction-effect",
            ("chance", Probability),
            ("entname", prototype.Index<EntityPrototype>(Entity).Name),
            ("amount", MaxEntities));
}
