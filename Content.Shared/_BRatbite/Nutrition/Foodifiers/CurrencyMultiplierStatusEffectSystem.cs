using Content.Shared._BRatbite.ServerCurrency;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class CurrencyMultiplierStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CurrencyMultiplierStatusEffectComponent, StatusEffectRelayedEvent<ServerCurrencyMultiplierEvent>>(OnServerCurrencyMultiplier);
        SubscribeLocalEvent<CurrencyMultiplierStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnServerCurrencyMultiplier(Entity<CurrencyMultiplierStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ServerCurrencyMultiplierEvent> args)
    {
        args.Args = args.Args with { Multiplier = args.Args.Multiplier * ent.Comp.Multiplier };
    }

    private void OnGetDescription(Entity<CurrencyMultiplierStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-server-currency-multiplier", ("multiplier", MathF.Round(ent.Comp.Multiplier * 100))));
        args.Message.PushNewline();
    }
}
