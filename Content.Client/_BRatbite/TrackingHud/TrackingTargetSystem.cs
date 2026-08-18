using Content.Shared._BRatbite.TrackingHud;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._BRatbite.TrackingHud;

public sealed partial class TrackingTargetSystem : SharedTrackingTargetSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    private TrackingTargetOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TargetTrackerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TargetTrackerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TargetTrackerComponent, LocalPlayerAttachedEvent>(OnAttach);
        SubscribeLocalEvent<TargetTrackerComponent, LocalPlayerDetachedEvent>(OnDetach);
    }

    private void OnInit(Entity<TargetTrackerComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<TargetTrackerComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnAttach(Entity<TargetTrackerComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnDetach(Entity<TargetTrackerComponent> ent, ref LocalPlayerDetachedEvent args)
    {

        _overlayManager.RemoveOverlay(_overlay);
    }

}
