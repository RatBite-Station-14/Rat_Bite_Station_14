using System.Numerics;
using Content.Shared._BRatbite.TrackingHud;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._BRatbite.TrackingHud;

public sealed partial class TrackingTargetOverlay : Overlay
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    internal TrackingTargetOverlay()
    {
        IoCManager.InjectDependencies(this);

    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entity.TryGetComponent<TargetTrackerComponent>(_playerManager.LocalEntity, out var tracker)) return;
        if (args.Viewport.Eye is not { } eye) return;
        var _sprite = _entity.System<SpriteSystem>();
        var arrowSprite = new SpriteSpecifier.Rsi(new("/Textures/_BRatBites/Interface/Misc/arrow.rsi/"), "arrow");
        foreach (var (_, target) in tracker.Targets)
        {
            if (target.MapId != args.MapId) continue;
            if (!_proto.TryIndex(target.MarkerPrototype, out var markerProto)) return;
            var eyePosition = eye.Position;
            float worldGap = 200f * eye.Zoom.X / EyeManager.PixelsPerMeter;
            var direction = target.TargetLocation - eyePosition.Position;

            var local = ClampMagnitude(direction, worldGap) + eyePosition.Position;
            var texture = _sprite.GetFrame(markerProto.Icon, _timing.RealTime);
            var iconSize = new Vector2(25, 25) * eye.Zoom.X / EyeManager.PixelsPerMeter;
            args.WorldHandle.DrawTextureRect(
                texture,
                new Box2Rotated(
                    Box2.FromDimensions(
                        local - iconSize / 2,
                        iconSize
                    ),
                    -eye.Rotation, local),
                target.PinColor
            );

            if (direction.LengthSquared() >= worldGap * worldGap)
            {
                var angle = Angle.FromWorldVec(-direction);
                var arrowTexture = _sprite.GetFrame(arrowSprite, _timing.RealTime);
                var arrowSize = new Vector2(16f, 16f) * eye.Zoom.X / EyeManager.PixelsPerMeter;
                var arrowCenter = local + angle.RotateVec(new Vector2(0f, 1f)) * iconSize * MathF.Sqrt(2) / 2;
                args.WorldHandle.DrawTextureRect(
                    arrowTexture,
                    new Box2Rotated(
                        Box2.FromDimensions(
                         arrowCenter - arrowSize / 2, arrowSize),
                        angle, arrowCenter),
                    target.PinColor
                );
            }
        }
    }

    private static Vector2 ClampMagnitude(Vector2 vec, float maxLength)
    {
        float sqrMagnitude = vec.LengthSquared();

        if (sqrMagnitude <= maxLength * maxLength)
            return vec;

        float magnitude = MathF.Sqrt(sqrMagnitude);
        float scale = maxLength / magnitude;

        return new(vec.X * scale, vec.Y * scale);
    }
}
