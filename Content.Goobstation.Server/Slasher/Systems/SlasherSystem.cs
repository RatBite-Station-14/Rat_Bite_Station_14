using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Server.Body.Components;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Moves the slasher's brain from the head into the chest
/// </summary>
public sealed class SlasherSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnMapInit(Entity<SlasherComponent> ent, ref MapInitEvent args) =>
        MoveBrainToChest(ent);

    private void OnRejuvenate(Entity<SlasherComponent> ent, ref RejuvenateEvent args) =>
        MoveBrainToChest(ent);

    private void MoveBrainToChest(Entity<SlasherComponent> ent)
    {
        if (!TryComp<BodyComponent>(ent, out var bodyComp)
            || !_body.TryGetBodyOrganEntityComps<BrainComponent>((ent.Owner, bodyComp), out var brains)
            || brains.Count == 0)
            return;

        var brain = brains[0];
        var slotId = brain.Comp2.SlotId;

        // Some species (e.g. slimes) don't use a standard "brain" organ slot.
        // Trying to force those organs into a brain slot asserts in debug.
        if (slotId != "brain")
            return;

        EntityUid? chestPart = null;
        foreach (var (partId, part) in _body.GetBodyChildrenOfType(ent.Owner, BodyPartType.Chest, bodyComp))
        {
            chestPart = partId;
            break;
        }

        if (chestPart == null)
            return;

        _body.RemoveOrgan(brain.Owner, brain.Comp2);
        _body.TryCreateOrganSlot(chestPart, slotId, out _, null);
        _body.InsertOrgan(chestPart.Value, brain.Owner, slotId);
        _standing.Stand(ent.Owner, force: true);
    }
}
