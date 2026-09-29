using Content.Shared._BRatbite.Nutrition;
using Content.Shared.Movement.Components;

namespace Content.Shared._BRatbite.Movement;

public sealed partial class IgnoreKudzuSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IgnoreKudzuComponent, EffectDescriptionEvent>(OnEffectDescription);
    }

    private void OnEffectDescription(Entity<IgnoreKudzuComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-ignore-kudzu"));
        args.Message.PushNewline();
    }
}
