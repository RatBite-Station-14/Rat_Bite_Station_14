using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class HungerMultiplierStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly HungerSystem _hungerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HungerMultiplierStatusEffectComponent, StatusEffectAppliedEvent>((_, ref ev) => RefreshHunger(ev.Target));
        SubscribeLocalEvent<HungerMultiplierStatusEffectComponent, StatusEffectRemovedEvent>((_, ref ev) => RefreshHunger(ev.Target));
        SubscribeLocalEvent<HungerMultiplierStatusEffectComponent, StatusEffectRelayedEvent<HungerMultiplierEvent>>(OnHungerMultiplier);
        SubscribeLocalEvent<HungerMultiplierStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void RefreshHunger(EntityUid ent)
    {
        // Force hunger to refresh
        _hungerSystem.DoHungerThresholdEffects(ent, null, true);
    }

    private void OnHungerMultiplier(Entity<HungerMultiplierStatusEffectComponent> ent, ref StatusEffectRelayedEvent<HungerMultiplierEvent> args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        args.Args = args.Args with { Multiplier = args.Args.Multiplier * ent.Comp.Multiplier * scale };
    }

    private void OnGetDescription(Entity<HungerMultiplierStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-hunger", ("multiplier", MathF.Round(ent.Comp.Multiplier * 100))));
        args.Message.PushNewline();
    }
}
