using Content.Goobstation.Common.Changeling;
using Content.Shared.EntityEffects;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.EntityEffects.Effects;

public sealed partial class DehuskEffect : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        if(!args.EntityManager.TryGetComponent<AbsorbedComponent>(args.TargetEntity, out var absorbedComponent)) return;
	if (!absorbedComponent.CanDehusk || absorbedComponent.Dehusked) return;

        absorbedComponent.Dehusked = true;
        args.EntityManager.RemoveComponent<UnrevivableComponent>(args.TargetEntity);
        var popup = args.EntityManager.EntitySysManager.GetEntitySystem<SharedPopupSystem>();
        popup.PopupEntity(Loc.GetString("dehusk-effect-popup"), args.TargetEntity);
    }
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString("dehusk-effect-guidebook-text");
}
