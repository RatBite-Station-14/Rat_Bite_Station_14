using Content.Goobstation.Common.Changeling;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.EntityEffects.Effects;

public sealed partial class DehuskEffect : EntityEffectBase<DehuskEffect>
{
    public sealed class DehuskEffectSystem : EntityEffectSystem<AbsorbedComponent, DehuskEffect>
    {
        protected override void Effect(Entity<AbsorbedComponent> entity, ref EntityEffectEvent<DehuskEffect> args)
        {
            if (!entity.Comp.CanDehusk || entity.Comp.Dehusked)
                return;

            entity.Comp.Dehusked = true;
            RemComp<UnrevivableComponent>(entity.Owner);
            _popup.PopupEntity(Loc.GetString("dehusk-effect-popup"), entity.Owner);
        }

        [Dependency] private readonly SharedPopupSystem _popup = default!;
    }

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString("dehusk-effect-guidebook-text");
}
