using Content.Ratbite.Shared.ItemMiner;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Station.Components;

namespace Content.Ratbite.Server.ItemMiner;

public sealed partial class TelecrystalMinerSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private NavMapSystem _nav = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecrystalMinerComponent, ItemMinerCheckEvent>(OnCheck);
        SubscribeLocalEvent<TelecrystalMinerComponent, ItemMinedEvent>(OnMined);
    }

    private void OnCheck(Entity<TelecrystalMinerComponent> ent, ref ItemMinerCheckEvent args)
    {
        // no worky on non-main stations
        if (!HasComp<BecomesStationComponent>(Transform(ent).ParentUid))
            args.Cancelled = true;
    }

    private void OnMined(Entity<TelecrystalMinerComponent> ent, ref ItemMinedEvent args)
    {
        ent.Comp.Accumulated += args.Count;

        switch (ent.Comp.NotifiedStage)
        {
            case TCMinerStage.Initial:
                {
                    if (ent.Comp.Accumulated >= ent.Comp.AnnounceAt)
                    {
                        ent.Comp.NotifiedStage = TCMinerStage.FirstAnnounced;

                        _chat.DispatchStationAnnouncement(
                            Transform(ent).ParentUid,
                            Loc.GetString(ent.Comp.Announcement),
                            playDefaultSound: true,
                            colorOverride: Color.Yellow
                        );
                    }
                    break;
                }
            case TCMinerStage.FirstAnnounced:
                {
                    if (ent.Comp.Accumulated >= ent.Comp.LocationAt)
                    {
                        ent.Comp.NotifiedStage = TCMinerStage.LocationAnnounced;

                        var xform = Transform(ent);
                        if (!_nav.TryGetNearestBeacon((ent, xform), out var beacon, out _))
                            return;
                        var nearest = beacon?.Comp?.Text!;

                        _chat.DispatchStationAnnouncement(
                            xform.ParentUid,
                            Loc.GetString(ent.Comp.LocationAnnouncement, ("location", nearest)),
                            playDefaultSound: true,
                            colorOverride: Color.Yellow
                        );
                    }
                    break;
                }
            default:
                break;
        }
    }
}
