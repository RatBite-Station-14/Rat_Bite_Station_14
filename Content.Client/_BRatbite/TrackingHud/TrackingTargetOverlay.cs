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
        if (args.Viewport.Eye is not { } eye) return;

        var _sprite = _entity.System<SpriteSystem>();

        foreach (var (_, target) in tracker.Targets)
        {
            if (target.MapId != args.MapId) continue;
            var eyePosition = eye.Position;
            float worldGap = 200f * eye.Zoom.X / EyeManager.PixelsPerMeter;
            var local = viewportControl.WorldToScreen(
                ClampMagnitude(target.TargetLocation - eyePosition.Position, worldGap) + eyePosition.Position
            );
            var texture = _sprite.GetFrame(target.Sprite, _timing.RealTime);
            var iconSize = new Vector2(100f, 100f);
            args.ScreenHandle.DrawTextureRect(texture,
                                              UIBox2.FromDimensions(
                                                  local - iconSize / 2,
                                                  iconSize),
                                              target.PinColor);
        }
    }

    private static Vector2 ClampMagnitude(Vector2 vec, float maxLength)
    {
        float sqrMagnitude = vec.X * vec.X + vec.Y * vec.Y;

        if (sqrMagnitude <= maxLength * maxLength)
            return vec;

        float magnitude = MathF.Sqrt(sqrMagnitude);
        float scale = maxLength / magnitude;

        return new(vec.X * scale, vec.Y * scale);
    }
}
