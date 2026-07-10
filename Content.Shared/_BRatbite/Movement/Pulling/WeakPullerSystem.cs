using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Movement.Pulling;

public sealed partial class WeakPullerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WeakPullerComponent, PullAttemptEvent>(OnPullAttempt);
    }

    private void OnPullAttempt(Entity<WeakPullerComponent> ent, ref PullAttemptEvent args)
    {
        if (args.Cancelled) return;
        // Weak pulles cannot pull non items
        if (!TryComp<ItemComponent>(args.PulledUid, out var itemComp) || !_proto.TryIndex(itemComp.Size, out var itemSize) || !_proto.TryIndex(ent.Comp.MaxPullableSize, out var maxPullableSize) || itemSize > maxPullableSize)
        {
            args.Cancelled = true;
            _popup.PopupClient(Loc.GetString("weak-puller-too-big"), ent.Owner, ent.Owner);
            return;
        }
    }
}
