// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Store.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._BRatbite.PermaBrig;
using Content.Shared.Actions;
using Content.Shared.GameTicking;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.PermaBrig;

public sealed class PermaUplinkSystem : EntitySystem
{
    private static readonly ProtoId<CurrencyPrototype> PermaPoint = "PermaPoint";

    private static readonly ProtoId<StoreCategoryPrototype>[] Categories =
    [
        "PermaEntertainment",
        "PermaBotany",
        "PermaMusic",
    ];

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrisonerComponent, ComponentStartup>(OnPrisonerStartup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PermaUplinkComponent, ComponentShutdown>(OnUplinkShutdown);
        SubscribeLocalEvent<PermaUplinkComponent, OpenPermaUplinkEvent>(OnOpenUplink);
    }

    private void OnPrisonerStartup(Entity<PrisonerComponent> ent, ref ComponentStartup args)
    {
        EnsurePermaUplink(ent);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.JobId != "Prisoner")
            return;

        EnsurePermaUplink(args.Mob);
    }

    private void EnsurePermaUplink(EntityUid uid)
    {
        var store = EnsureComp<StoreComponent>(uid);
        store.Name = "store-preset-name-perma-uplink";
        store.CurrencyWhitelist.Add(PermaPoint);

        foreach (var category in Categories)
        {
            store.Categories.Add(category);
        }

        store.Balance.TryAdd(PermaPoint, FixedPoint2.New(5));
        _store.RefreshAllListings(store);

        var uplink = EnsureComp<PermaUplinkComponent>(uid);
        if (uplink.ActionEntity == null)
            _actions.AddAction(uid, ref uplink.ActionEntity, uplink.Action);

        Dirty(uid, uplink);
    }

    private void OnUplinkShutdown(Entity<PermaUplinkComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ActionEntity == null)
            return;

        _actions.RemoveAction(ent.Comp.ActionEntity);
        ent.Comp.ActionEntity = null;
    }

    private void OnOpenUplink(Entity<PermaUplinkComponent> ent, ref OpenPermaUplinkEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
        args.Handled = true;
    }
}
