using System.Numerics;
using Content.Shared._BRatbite.Silicon;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._BRatbite.Silicon;

public sealed class CoreSquishedSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoreSquishedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CoreSquishedComponent, ComponentRemove>(OnRemove);
    }

    private void OnStartup(Entity<CoreSquishedComponent> ent, ref ComponentStartup args)
    {
        ApplyScale(ent);
    }

    private void OnRemove(Entity<CoreSquishedComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.OriginalScale is not { } originalScale ||
            !TryComp(ent, out SpriteComponent? sprite))
        {
            return;
        }

        _sprite.SetScale((ent.Owner, sprite), originalScale);
    }

    private void ApplyScale(Entity<CoreSquishedComponent> ent)
    {
        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        ent.Comp.OriginalScale ??= sprite.Scale;
        var elapsed = _timing.CurTime - ent.Comp.RecoveryStart;
        var progress = ent.Comp.RecoveryDuration > TimeSpan.Zero
            ? Math.Clamp((float) (elapsed / ent.Comp.RecoveryDuration), 0f, 1f)
            : 1f;
        var recoveryScale = Vector2.Lerp(ent.Comp.SquishScale, Vector2.One, progress);
        _sprite.SetScale((ent.Owner, sprite), ent.Comp.OriginalScale.Value * recoveryScale);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CoreSquishedComponent>();
        while (query.MoveNext(out var uid, out var squished))
            ApplyScale((uid, squished));
    }
}
