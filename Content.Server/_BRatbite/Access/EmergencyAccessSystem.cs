using Content.Server.AlertLevel;
using Content.Server.Station.Systems;
using Content.Shared._BRatbite.Access;
using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.Access;

public sealed partial class EmergencyAccessSystem : SharedEmergencyAccessSystem
{
    [Dependency] private readonly AlertLevelSystem _stationAlertLevelSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EmergencyAccessComponent, MapInitEvent>(OnMapInit);
        base.Initialize();
    }

    protected override bool IsAlertLevelReached(Entity<EmergencyAccessComponent> ent)
    {
        if (_stationSystem.GetOwningStation(ent.Owner) is not { } station)
        {
            return false;
        }
        var level = _stationAlertLevelSystem.GetLevel(station);
        ent.Comp.CurrentAlertLevel = level;
        DirtyField<EmergencyAccessComponent>(ent.Owner, ent.Comp, nameof(EmergencyAccessComponent.CurrentAlertLevel));
        return level == ent.Comp.TargetAlert;
    }

    private void OnMapInit(Entity<EmergencyAccessComponent> ent, ref MapInitEvent args)
    {
        var addGroup = (HashSet<ProtoId<AccessGroupPrototype>> Group, HashSet<ProtoId<AccessLevelPrototype>> Tags) =>
        {
            foreach (var group in Group)
            {
                if (!_proto.TryIndex(group, out var proto))
                    continue;
                Tags.UnionWith(proto.Tags);
            }
        };
        addGroup(ent.Comp.AddedGroups, ent.Comp.AddedTags);
        addGroup(ent.Comp.RemovedGroups, ent.Comp.RemovedTags);
        Dirty(ent);
    }
}
