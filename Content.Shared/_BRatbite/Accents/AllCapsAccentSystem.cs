using Content.Shared._BRatbite.Nutrition;
using Content.Shared.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Accents;

public sealed class AllCapsAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AllCapsAccentComponent, AccentGetEvent>(OnGetAccent);
        SubscribeLocalEvent<AllCapsAccentComponent, StatusEffectRelayedEvent<AccentGetEvent>>(OnGetAccentStatusEffect);
        SubscribeLocalEvent<AllCapsAccentComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private string Accentuate(Entity<AllCapsAccentComponent> ent, string message)
    {
        return $"{message.ToUpper()}";
    }

    private void OnGetAccent(Entity<AllCapsAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(ent, args.Message);
    }

    private void OnGetAccentStatusEffect(Entity<AllCapsAccentComponent> ent, ref StatusEffectRelayedEvent<AccentGetEvent> args)
    {
        args.Args.Message = Accentuate(ent, args.Args.Message);
    }

    private void OnGetDescription(Entity<AllCapsAccentComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-all-caps"));
        args.Message.PushNewline();
    }
}
