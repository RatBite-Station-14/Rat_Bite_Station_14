using Content.Server.Explosion.EntitySystems;
using Content.Shared.Body.Systems;
using Content.Shared.Examine;

namespace Content.Server._BRatbite.Traits;

public sealed partial class TriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    private HashSet<EntityUid> queuedEntities = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TriggerComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        foreach (var uid in queuedEntities)
        {
            if (TerminatingOrDeleted(uid)) continue;
            if (!TryComp<TriggerComponent>(uid, out var component)) continue;
            _explosionSystem.QueueExplosion(
                _transformSystem.GetMapCoordinates(uid),
                ExplosionSystem.DefaultExplosionPrototypeId,
                component.TotalIntensity,
                component.Slope,
                component.MaxTileIntensity,
                uid
            );

            _bodySystem.GibBody(uid);
        }
        queuedEntities.Clear();
    }

    private void OnExamined(Entity<TriggerComponent> ent, ref ExaminedEvent args)
    {
        if (args.Examined == args.Examiner) return;
        if (!args.IsInDetailsRange) return;
        // Defer explosion to next frame otherwise we get weird errors
        queuedEntities.Add(ent.Owner);
    }
}

