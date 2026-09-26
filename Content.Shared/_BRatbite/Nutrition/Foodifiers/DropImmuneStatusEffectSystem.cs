using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class DropImmuneStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DropImmuneStatusEffectComponent, StatusEffectRelayedEvent<FellDownThrowAttemptEvent>>(OnDropThrowAttempt);
        SubscribeLocalEvent<DropImmuneStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnDropThrowAttempt(Entity<DropImmuneStatusEffectComponent> ent, ref StatusEffectRelayedEvent<FellDownThrowAttemptEvent> args)
    {
        args.Args = args.Args with { Cancelled = true };
    }

    private void OnGetEffectDescription(Entity<DropImmuneStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-drop-immune"));
        args.Message.PushNewline();
    }
}
