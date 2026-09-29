using Content.Shared.Overlays;
using Content.Shared.Radiation.Components;

namespace Content.Shared._BRatbite.Nutrition;

public sealed partial class EffectDescriptionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        // Effects where they work out of the box with status effects,
        // and the only thing to add is the description event, we put
        // it here to minimize potential collisions
        SubscribeLocalEvent<RadiationSourceComponent, EffectDescriptionEvent>(OnGetRadiationDescription);
        SubscribeLocalEvent<ShowJobIconsComponent, EffectDescriptionEvent>(OnGetJobIconsDescription);
    }

    private void OnGetRadiationDescription(Entity<RadiationSourceComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-radiation", ("weak", ent.Comp.IsWeakSource), ("intensity", MathF.Round(ent.Comp.Intensity, 2)), ("slope", ent.Comp.Slope)));
        args.Message.PushNewline();
    }

    private void OnGetJobIconsDescription(Entity<ShowJobIconsComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-job-icons"));
        args.Message.PushNewline();
    }
}
