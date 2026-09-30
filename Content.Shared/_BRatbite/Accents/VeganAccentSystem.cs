using Content.Shared._BRatbite.Nutrition;
using Content.Shared.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Accents;

public sealed partial class VeganAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VeganAccentComponent, AccentGetEvent>(OnAccentGet);
        SubscribeLocalEvent<VeganAccentComponent, StatusEffectRelayedEvent<AccentGetEvent>>(OnAccentGetStatusEffect);
        SubscribeLocalEvent<VeganAccentComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnAccentGet(Entity<VeganAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(ent, args.Message);
    }

    private void OnAccentGetStatusEffect(Entity<VeganAccentComponent> ent, ref StatusEffectRelayedEvent<AccentGetEvent> args)
    {
        args.Args.Message = Accentuate(ent, args.Args.Message);
    }

    private string Accentuate(Entity<VeganAccentComponent> ent, string message)
    {
        return $"{message}... {Loc.GetString("accent-vegan-btw")}";
    }

    private void OnGetDescription(Entity<VeganAccentComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-vegan"));
        args.Message.PushNewline();
    }
}
