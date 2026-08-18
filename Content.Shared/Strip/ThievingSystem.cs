// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Strip.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Strip;

public sealed partial class ThievingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThievingComponent, BeforeStripEvent>(OnBeforeStrip);
        SubscribeLocalEvent<ThievingComponent, InventoryRelayedEvent<BeforeStripEvent>>((e, c, ev) =>
            OnBeforeStrip(e, c, ev.Args));
        SubscribeLocalEvent<ThievingComponent, ToggleThievingEvent>(OnToggleStealthy);
        SubscribeLocalEvent<ThievingComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<ThievingComponent, ComponentRemove>(OnCompRemoved);
    }

    private void OnBeforeStrip(EntityUid uid, ThievingComponent component, BeforeStripEvent args)
    {
        args.Stealth |= component.Stealthy;
        args.Subtle |= component.Subtle;
        if (args.Stealth)
        {
            args.Additive -= component.StripTimeReduction;
        }

        args.Multiplier *= component.TimeMultiplier;

        if (!component.TraitGranted && HasComp<ThievingTraitComponent>(uid))
            args.Multiplier *= ThievingTraitComponent.StripTimeMultiplier;
    }

    private void OnCompInit(Entity<ThievingComponent> entity, ref ComponentInit args)
    {
        _alertsSystem.ShowAlert(entity.Owner, entity.Comp.StealthyAlertProtoId, 1);
    }

    private void OnCompRemoved(Entity<ThievingComponent> entity, ref ComponentRemove args)
    {
        _alertsSystem.ClearAlert(entity.Owner, entity.Comp.StealthyAlertProtoId);
    }

    private void OnToggleStealthy(Entity<ThievingComponent> ent, ref ToggleThievingEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.ToggleSubtle)
        {
            ent.Comp.Subtle = !ent.Comp.Subtle;
            DirtyField(ent.AsNullable(), nameof(ent.Comp.Subtle), null);
        }
        else
        {
            ent.Comp.Stealthy = !ent.Comp.Stealthy;
            DirtyField(ent.AsNullable(), nameof(ent.Comp.Stealthy), null);
        }

        _alertsSystem.ShowAlert(ent.Owner, ent.Comp.StealthyAlertProtoId, GetAlertState(ent.Comp));

        args.Handled = true;
    }

    private static short GetAlertState(ThievingComponent component)
    {
        var enabled = component.ToggleSubtle ? component.Subtle : component.Stealthy;
        return (short)(enabled ? 1 : 0);
    }
}
