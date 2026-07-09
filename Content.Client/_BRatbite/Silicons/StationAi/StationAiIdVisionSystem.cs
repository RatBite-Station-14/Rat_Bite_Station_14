using Content.Client._BRatbite.Humanoid;
using Content.Client.Clothing;
using Content.Client.Humanoid;
using Content.Goobstation.Common.Clothing;
using Content.Shared._BRatbite.Traits;
using Content.Shared.Access.Systems;
using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Station;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client._BRatbite.Silicons.StationAi;

// ID based vision for station AI
public sealed partial class StationAiIdVisionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    // Map job prototype and slot -> layer
    private Dictionary<(ProtoId<JobPrototype>?, string), List<(string, PrototypeLayerData)>> clothingCache = new();
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawningSystem = default!;
    [Dependency] private readonly ClientClothingSystem _clothingSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HumanoidAppearanceComponent, BeforeGetHumanoidAppearanceEvent>(OnBeforeGetHumanoidAppearance);
        SubscribeLocalEvent<HumanoidAppearanceComponent, AttemptHumanoidMarkingEvent>(OnHumanoidAttemptMarkingEvent);
        SubscribeLocalEvent<HumanoidAppearanceComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisual);
        SubscribeLocalEvent<HumanoidAppearanceComponent, JobChangeEvent>(OnJobChange);
        SubscribeLocalEvent<HumanoidAppearanceComponent, CheckClothingSlotHiddenEvent>(OnHideClothingCheck);
        SubscribeLocalEvent<FaceBlindComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<FaceBlindComponent, LocalPlayerDetachedEvent>(OnDetached);
        SubscribeLocalEvent<FaceBlindComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FaceBlindComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnBeforeGetHumanoidAppearance(Entity<HumanoidAppearanceComponent> ent, ref BeforeGetHumanoidAppearanceEvent args)
    {
        if (!_isActive) return;
        args.Species = "Human";
        args.Height = 1f;
        args.Width = 1f;
        args.EyeColor = new Color(0f, 0f, 0f);
        // Everyone is emoji yellow
        args.SkinColor = new Color(0xFF, 0xDE, 0x34);
    }

    private void OnHumanoidAttemptMarkingEvent(Entity<HumanoidAppearanceComponent> ent, ref AttemptHumanoidMarkingEvent args)
    {
        if (!_isActive) return;
        args.Cancelled = true;
    }

    private void OnGetEquipmentVisual(Entity<HumanoidAppearanceComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!_isActive) return;
        args.Handled = true;
        args.SkipSpecies = true;

        var job = GetEntityJob((ent.Owner, EnsureComp<TrackJobChangeComponent>(ent)));
        if (clothingCache.TryGetValue((job, args.Slot), out var layer))
        {
            args.Layers = layer;
            return;
        }
        if (job == null)
        {
            return;
        }
        EntProtoId? clothingProtoId = null;
        foreach (var outfit in _proto.EnumeratePrototypes<ChameleonOutfitPrototype>())
        {
            if (outfit.Job == job && outfit.Equipment.TryGetValue(args.Slot, out var clothingId))
            {
                clothingProtoId = clothingId;
            }
        }

        if (clothingProtoId is null && _proto.TryIndex<RoleLoadoutPrototype>(LoadoutSystem.GetJobPrototype(job), out var roleLoadoutPrototype))
        {
            foreach (var group in roleLoadoutPrototype.Groups)
            {
                if (!_proto.TryIndex(group, out var loadoutGroup)) continue;
                foreach (var loadout in loadoutGroup.Loadouts)
                {
                    if (!_proto.TryIndex(loadout, out var loadoutPrototype)) continue;
                    if (loadoutPrototype.Equipment.TryGetValue(args.Slot, out var clothingId))
                    {
                        clothingProtoId = clothingId;
                        break;
                    }
                }
            }
        }

        if (clothingProtoId is null && _proto.TryIndex(job, out var jobStartingGear) && _proto.TryIndex(jobStartingGear.StartingGear, out var startingGearPrototype))
        {
            if (startingGearPrototype.Equipment.TryGetValue(args.Slot, out var clothingId))
            {
                clothingProtoId = clothingId;
            }
        }

        if (clothingProtoId is { } clothing)
        {
            var entity = Spawn(clothing, doMapInit: false);
            RaiseLocalEvent(entity, args);
            foreach (var (_, l) in args.Layers)
            {
                if (l.RsiPath == null && l.TexturePath == null && TryComp<SpriteComponent>(entity, out var sprite))
                {
                    l.RsiPath = sprite.BaseRSI?.Path.CanonPath;
                }
            }
            clothingCache.Add((job, args.Slot), args.Layers);
            Del(entity);
        }
        else
        {
            clothingCache.Add((job, args.Slot), new());
        }
        return;
    }

    private ProtoId<JobPrototype>? GetEntityJob(Entity<TrackJobChangeComponent> ent)
    {
        if (ent.Comp.LastJobIcon is not { } lastJobIcon || _proto.EnumeratePrototypes<JobPrototype>().FirstOrDefault((j) => j.Icon == lastJobIcon) is not { } job) return null;
        return job;
    }

    private void OnJobChange(Entity<HumanoidAppearanceComponent> ent, ref JobChangeEvent args)
    {
        _clothingSystem.UpdateAllSlots(ent);
    }

    private bool _isActive = false;

    private void OnDetached(Entity<FaceBlindComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _isActive = false;
        RedrawAllEntities();
    }

    private void OnAttached(Entity<FaceBlindComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _isActive = true;
        RedrawAllEntities();
    }

    private void RedrawAllEntities()
    {
        var humanoidQuery = AllEntityQuery<HumanoidAppearanceComponent, SpriteComponent>();
        while (humanoidQuery.MoveNext(out var uid, out var humanoidComp, out var spriteComp))
        {
            _humanoidAppearanceSystem.UpdateSprite((uid, humanoidComp, spriteComp));
            _clothingSystem.UpdateAllSlots(uid);
        }
    }

    private void OnInit(Entity<FaceBlindComponent> ent, ref ComponentInit args)
    {
        if (ent == _playerManager.LocalEntity)
        {
            _isActive = true;
            RedrawAllEntities();
        }
    }

    private void OnShutdown(Entity<FaceBlindComponent> ent, ref ComponentShutdown args)
    {
        if (ent == _playerManager.LocalEntity)
        {
            _isActive = false;
            RedrawAllEntities();
        }
    }

    private void OnHideClothingCheck(Entity<HumanoidAppearanceComponent> ent, ref CheckClothingSlotHiddenEvent args)
    {
        if (_isActive) args.Handled = true;
    }
}
