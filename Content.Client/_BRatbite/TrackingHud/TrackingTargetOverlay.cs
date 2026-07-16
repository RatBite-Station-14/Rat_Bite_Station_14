using System.Numerics;
using Content.Shared._BRatbite.TrackingHud;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

namespace Content.Client._BRatbite.TrackingHud;

public sealed partial class TrackingTargetOverlay : Overlay
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    internal TrackingTargetOverlay()
    {
        IoCManager.InjectDependencies(this);

    }

    protected override void Draw(in OverlayDrawArgs args)
    {

        if (!_entity.TryGetComponent<TargetTrackerComponent>(_playerManager.LocalEntity, out var tracker)) return;
        if (args.ViewportControl is not { } viewportControl)
            return;
        var _sprite = _entity.System<SpriteSystem>();

        foreach (var (_, target) in tracker.Targets)
        {
            if (target.MapId != args.MapId) continue;
            var local = viewportControl.WorldToScreen(target.TargetLocation);
            var size = viewportControl.WorldToScreen(args.WorldBounds.BottomRight);

            var gap = new Vector2(230f, 120f);
            var texture = _sprite.GetFrame(target.Sprite, _timing.RealTime);
            var iconSize = new Vector2(100f, 100f);
            args.ScreenHandle.DrawTextureRect(texture,
                                              UIBox2.FromDimensions(
                                                  Vector2.Clamp(local, Vector2.Zero + gap, size - gap) - (iconSize / 2),
                                                  iconSize),
                                              target.PinColor);
        }
    }
}
