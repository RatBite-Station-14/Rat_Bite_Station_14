using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared._BRatbite.Plushies;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.Plushies;

public sealed class BatchSmiteOnHitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ExplosionSystem _explosions = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BatchSmiteOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BatchSmiteOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (!_random.Prob(ent.Comp.TriggerChance))
            return;

        if (_random.Prob(ent.Comp.SelfExplodeChance))
        {
            ExplodeSmite(args.User);
            return;
        }

        var target = args.HitEntities[0];
        if (TerminatingOrDeleted(target))
            return;

        _body.GibBody(target);
    }

    private void ExplodeSmite(EntityUid victim)
    {
        if (TerminatingOrDeleted(victim))
            return;

        var coords = _transform.GetMapCoordinates(victim);
        _explosions.QueueExplosion(
            coords,
            ExplosionSystem.DefaultExplosionPrototypeId,
            4,
            1,
            2,
            victim,
            maxTileBreak: 0);

        _body.GibBody(victim);
    }
}
