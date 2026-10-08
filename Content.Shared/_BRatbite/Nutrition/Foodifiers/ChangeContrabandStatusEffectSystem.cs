using Content.Shared.Contraband;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed class ChangeContrabandStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeContrabandStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<ChangeContrabandStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);
        SubscribeLocalEvent<ChangeContrabandStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnStatusApplied(Entity<ChangeContrabandStatusEffectComponent> entity,
        ref StatusEffectAppliedEvent args)
    {
        if (TryComp<ContrabandComponent>(args.Target, out var contrabandComponent))
        {
            entity.Comp.EntityHadContrabandComponent = true;

            entity.Comp.OldSeverity = contrabandComponent.Severity;
            entity.Comp.OldAllowedDepartments = contrabandComponent.AllowedDepartments;
            entity.Comp.OldAllowedJobs = contrabandComponent.AllowedJobs;

            contrabandComponent.Severity = entity.Comp.Severity;
            contrabandComponent.AllowedDepartments = entity.Comp.AllowedDepartments;
            contrabandComponent.AllowedJobs = entity.Comp.AllowedJobs;
        }
        else
        {
            var newContrabandComponent = EnsureComp<ContrabandComponent>(args.Target);

            newContrabandComponent.Severity = entity.Comp.Severity;
            newContrabandComponent.AllowedDepartments = entity.Comp.AllowedDepartments;
            newContrabandComponent.AllowedJobs = entity.Comp.AllowedJobs;
        }
    }

    private void OnStatusRemoved(Entity<ChangeContrabandStatusEffectComponent> entity,
        ref StatusEffectRemovedEvent args)
    {
        if (entity.Comp.EntityHadContrabandComponent && TryComp<ContrabandComponent>(args.Target, out var contrabandComponent))
        {
            // if EntityHadContrabandComponent is true, all those things shouldn't be null
            contrabandComponent.Severity = entity.Comp.OldSeverity!.Value;
            contrabandComponent.AllowedDepartments = entity.Comp.OldAllowedDepartments!;
            contrabandComponent.AllowedJobs = entity.Comp.OldAllowedJobs!;
        }
        else
        {
            RemCompDeferred<ContrabandComponent>(args.Target);
        }
    }

    private void OnGetDescription(Entity<ChangeContrabandStatusEffectComponent> entity, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-change-contraband"));
        args.Message.PushNewline();
    }
}
