using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Server.Storage.Components;
using Content.Shared._BRatbite.PermaBrig;
using Content.Shared.Cuffs;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.Spawners;

public sealed partial class StowawaySystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffableSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("stowaway");

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawn, before: new[] { typeof(ArrivalsSystem) });
        SubscribeLocalEvent<StowawayComponent, MapInitEvent>(OnStowawayInit);
    }

    private void OnPlayerSpawn(PlayerSpawningEvent args)
    {
        // Skip station AIs
        if (args.Job == "StationAi") return;
        if (!args.HumanoidCharacterProfile?.TraitPreferences?.Contains("Stowaway") ?? false)
            return;
        if (args.SpawnResult != null)
        {
            _sawmill.Warning($"Entity {args.SpawnResult} has StowawayComponent but was already spawned");
            return;
        }
        var query = EntityQueryEnumerator<StowawaySpawnPointComponent, ContainerManagerComponent, TransformComponent>();
        var closets = new List<Entity<StowawaySpawnPointComponent, ContainerManagerComponent, TransformComponent>>();
        while (query.MoveNext(out var uid, out var spawnPoint, out var container, out var transform))
        {
            if (args.Station != null && _station.GetOwningStation(uid, transform) != args.Station)
                continue;
            // Maybe in the future allow some jobs to be spawned in secure lockers
            if (spawnPoint.Secure) continue;
            closets.Add((uid, spawnPoint, container, transform));
        }
        if (closets.Count == 0)
            return;
        var spawnedEntity = _stationSpawning.SpawnPlayerMob(closets[0].Comp3.Coordinates, args.Job, args.HumanoidCharacterProfile, args.Station);
        args.SpawnResult = spawnedEntity;
        while (closets.Count != 0)
        {
            var (closetUid, spawnPoint, manager, xform) = _random.PickAndTake(closets);
            if (!_container.TryGetContainer(closetUid, spawnPoint.ContainerId, out var container, manager))
                continue;
            if (TryComp<EntityStorageComponent>(closetUid, out var entityStorage) && entityStorage.Open)
                continue;

            if (!_container.Insert(args.SpawnResult.Value, container, containerXform: xform))
                continue;

            return;
        }
        Del(args.SpawnResult);
        args.SpawnResult = null;
    }

    private void OnStowawayInit(Entity<StowawayComponent> ent, ref MapInitEvent args)
    {
        if (HasComp<PrisonerComponent>(ent)) HandlePrisonerStowaway(ent);
        if (!TryComp<InventoryComponent>(ent, out var inventoryComp))
            return;
        foreach (var slot in inventoryComp.Slots)
        {
            if (!_random.Prob(ent.Comp.SlotDeletionChance)) continue;
            if (!_inventorySystem.TryUnequip(ent, slot.Name, out var removedItem, force: true, silent: true)) continue;
            Del(removedItem);
        }

    }
    private ProtoId<DamageGroupPrototype> _prisonerDamage = "Brute";
    private void HandlePrisonerStowaway(Entity<StowawayComponent> ent)
    {
        var cuffs = SpawnNextToOrDrop("HardZipties", ent);
        // Since prisoners can't be stowed away in lockers, they are cuffed and beat up
        // Since sec forgot about them
        _cuffableSystem.TryAddNewCuffs(ent, ent, cuffs);
        _damageableSystem.TryChangeDamage(ent, new DamageSpecifier(_proto.Index(_prisonerDamage), 90));
    }
}
