using Content.Shared._BRatbite.Nutrition;
using Content.Shared.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Accents;

public sealed partial class CopyPastaAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CopyPastaAccentComponent, AccentGetEvent>(OnGetAccent);
        SubscribeLocalEvent<CopyPastaAccentComponent, StatusEffectRelayedEvent<AccentGetEvent>>(OnGetAccentStatusEffect);
        SubscribeLocalEvent<CopyPastaAccentComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private string Accentuate(Entity<CopyPastaAccentComponent> ent, string message)
    {
        return $"{message} {message}";
    }

    private void OnGetAccent(Entity<CopyPastaAccentComponent> ent, ref AccentGetEvent args)
    {
        args.Message = Accentuate(ent, args.Message);
    }

    private void OnGetAccentStatusEffect(Entity<CopyPastaAccentComponent> ent, ref StatusEffectRelayedEvent<AccentGetEvent> args)
    {
        args.Args.Message = Accentuate(ent, args.Args.Message);
    }

    private void OnGetDescription(Entity<CopyPastaAccentComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-copy-pasta"));
        args.Message.PushNewline();
    }
}
