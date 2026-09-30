using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class GunAccuracyStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GunAccuracyStatusEffectComponent, StatusEffectRelayedEvent<GunRefreshModifiersEvent>>(OnGunRefresh);
        SubscribeLocalEvent<GunAccuracyStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnGunRefresh(Entity<GunAccuracyStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GunRefreshModifiersEvent> args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        var ev = args.Args;
        ev.MinAngle *= ent.Comp.AccuracyMultiplier * scale;
        ev.MaxAngle *= ent.Comp.AccuracyMultiplier * scale;
        args.Args = ev;
    }

    private void OnGetEffectDescription(Entity<GunAccuracyStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-change-accuracy", ("accuracy", MathF.Round(ent.Comp.AccuracyMultiplier * 100))));
        args.Message.PushNewline();
    }
}
