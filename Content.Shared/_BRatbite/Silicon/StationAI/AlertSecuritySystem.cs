using Content.Shared._BRatbite.TrackingHud;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.Silicon.StationAI;

public sealed partial class AlertSecuritySystem : EntitySystem
{
    [Dependency] private readonly SharedTrackingTargetSystem _trackingTargetSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    private int _counter = 0;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationAiHeldComponent, AlertSecurityActionEvent>(OnSecurityAction);
    }

    private void OnSecurityAction(Entity<StationAiHeldComponent> ent, ref AlertSecurityActionEvent args)
    {
        if (args.Handled) return;
        var pos = args.Target.Position;
        var coords = _transformSystem.ToMapCoordinates(args.Target);
        _trackingTargetSystem.RemoveFromAllTargets($"{args.DefaultId}-${_counter}");
        _counter++;
        _trackingTargetSystem.AddTargetToAllEntities(new TrackingTarget
        {
            TargetLocation = coords.Position,
            MapId = coords.MapId,
            PinColor = new Color(0f, 1f, 0f, 0.6f),
            Sprite = new SpriteSpecifier.Rsi(new("/Textures/Mobs/Silicon/station_ai.rsi"), "ai"),
            Channels = ListeningChannels.SECURITY | ListeningChannels.SILICON,
        }, defaultId: $"{args.DefaultId}-${_counter}", deleteAfter: TimeSpan.FromSeconds(30));

        args.Handled = true;
    }
}
