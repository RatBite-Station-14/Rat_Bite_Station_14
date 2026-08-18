using System.Numerics;
using Content.Client.Pointing.Components;
using Content.Shared._BRatbite.CCVar;
using Content.Shared.Pointing;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Client.Pointing;

public sealed partial class PointingSystem : SharedPointingSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public void SubscribeCVars()
    {
        _cfg.OnValueChanged(RatbiteCVars.PointerScale, value => ScaleAllPointers());
        _cfg.OnValueChanged(RatbiteCVars.PointerOutline, value => ScaleAllPointers());
    }

    private void ScaleAllPointers()
    {
        var enumerator = EntityQueryEnumerator<PointingArrowComponent, SpriteComponent>();
        while (enumerator.MoveNext(out var uid, out var pointer, out var sprite))
        {
            ScalePointer((uid, pointer, sprite));
        }
    }

    private void ScalePointer(Entity<PointingArrowComponent, SpriteComponent> ent)
    {
        _sprite.SetScale((ent, ent.Comp2), Vector2.One * _cfg.GetCVar(RatbiteCVars.PointerScale));
        var index = _sprite.LayerMapReserve((ent, ent.Comp2), ent.Comp1.OutlineLayer);
        _sprite.LayerSetVisible((ent, ent.Comp2), index, _cfg.GetCVar(RatbiteCVars.PointerOutline));
    }
}
