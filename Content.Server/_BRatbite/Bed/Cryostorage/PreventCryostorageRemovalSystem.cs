using Robust.Shared.Containers;

using Content.Shared.Bed.Cryostorage;
using Content.Shared.Administration.Logs;
using Content.Shared.Access.Systems;
using Content.Shared.Verbs;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Mind.Components;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.Bed.Cryostorage;

/// <summary>
/// Prevents players from ejecting AFK people from the cryostorage if they don't have
/// cryo access
/// </summary>
public sealed class PreventCryostorageRemovalSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly SharedContainerSystem _sharedContainerSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CryostorageComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
    }

    private void AddAlternativeVerbs(Entity<CryostorageComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (_sharedContainerSystem.TryGetContainer(ent.Owner, ent.Comp.ContainerId, out var baseContainer))
        {
            if (baseContainer.ContainedEntities.Count > 0)
            {
                var user = args.User;
                args.Verbs.Add(
                new AlternativeVerb
                {
                    Text = Loc.GetString("container-verb-text-empty"),
                    Category = VerbCategory.Eject,
                    Priority = 1,
                    Act = () =>
                    {
                        TryEjectBody(ent, user, baseContainer);
                    }
                }
                );
            }
        }
    }

    private void TryEjectBody(Entity<CryostorageComponent> ent, EntityUid userId, BaseContainer container)
    {
        if (container.ContainedEntities.Count == 0)
            return;
        var cryodEntity = container.ContainedEntities[0];
        var hasMind = _mind.TryGetMind(cryodEntity, out EntityUid mind, out var mindComponent);
        var session = mindComponent?.UserId;
        var hasActiveSession = session != null && _playerManager.ValidSessionId(session.Value);
        if ((!hasMind || !hasActiveSession) && !_accessReader.IsAllowed(userId, ent))
        {
            _popupSystem.PopupEntity(Loc.GetString("cryostorage-prevent-removal"), ent.Owner, userId);
            return;
        }

        _adminLog.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(userId):player} emptied container {ToPrettyString(ent)}");
        _sharedContainerSystem.EmptyContainer(container);
    }
}
