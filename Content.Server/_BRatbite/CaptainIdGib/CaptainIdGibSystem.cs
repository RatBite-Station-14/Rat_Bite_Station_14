using Content.Server.Body.Systems;
using Content.Shared.Hands.Components;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Server._BRatbite.CaptainIDGib;

public sealed class CaptainIDGibSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CaptainIDGibComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
    }

    private void OnInserted(Entity<CaptainIDGibComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        var picker = args.Container.Owner;
        if (!HasComp<HandsComponent>(picker) ||
            !_player.TryGetSessionByEntity(picker, out var session) ||
            !IsTarget(ent.Comp.TargetUserIds, session.UserId.UserId))
        {
            return;
        }

        _body.GibBody(picker);
    }

    private static bool IsTarget(HashSet<string> targetUserIds, Guid userId)
    {
        foreach (var id in targetUserIds)
        {
            if (Guid.TryParse(id, out var target) && target == userId)
                return true;
        }

        return false;
    }
}
