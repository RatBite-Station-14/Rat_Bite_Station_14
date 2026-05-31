// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
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
    }

    private void OnCompInit(Entity<ThievingComponent> entity, ref ComponentInit args)
    {
        _alertsSystem.ShowAlert(entity, entity.Comp.StealthyAlertProtoId, GetAlertState(entity.Comp));
    }

    private void OnCompRemoved(Entity<ThievingComponent> entity, ref ComponentRemove args)
    {
        _alertsSystem.ClearAlert(entity, entity.Comp.StealthyAlertProtoId);
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
