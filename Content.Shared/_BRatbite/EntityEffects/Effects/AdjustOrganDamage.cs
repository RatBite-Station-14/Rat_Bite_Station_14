using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared._BRatbite.EntityEffects.Effects;

public sealed partial class AdjustOrganDamage : EntityEffectBase<AdjustOrganDamage>
{
    [DataField(required: true)]
    public FixedPoint2 Amount = default!;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}

public sealed class AdjustOrganDamageEffectSystem : EntityEffectSystem<BodyComponent, AdjustOrganDamage>
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<AdjustOrganDamage> args)
    {
        var amount = args.Effect.Amount;
        var organs = _body.GetBodyOrgans(ent.Owner, ent.Comp).ToList();

        if (organs.Count == 0 || amount == 0)
            return;

        if (amount > 0)
        {
            var damage = amount / organs.Count;
            foreach (var organ in organs)
            {
                if (!_trauma.TryChangeOrganDamageModifier(organ.Id, damage, ent.Owner, "ReagentDamage", organ.Component))
                    _trauma.TryCreateOrganDamageModifier(organ.Id, damage, ent.Owner, "ReagentDamage", organ.Component);
            }

            return;
        }

        var healAmount = -amount;
        foreach (var organ in organs)
        {
            foreach (var modifier in organ.Component.IntegrityModifiers.ToArray())
            {
                if (healAmount >= modifier.Value)
                {
                    healAmount -= modifier.Value;
                    _trauma.TryRemoveOrganDamageModifier(organ.Id, modifier.Key.Item2, modifier.Key.Item1, organ.Component);

                    if (healAmount <= 0)
                        return;
                }
                else
                {
                    _trauma.TryChangeOrganDamageModifier(organ.Id, -healAmount, modifier.Key.Item2, modifier.Key.Item1, organ.Component);
                    return;
                }
            }
        }
    }
}
