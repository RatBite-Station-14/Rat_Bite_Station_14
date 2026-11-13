using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared._BRatbite.Inventory;

public sealed partial class UnremovaeableOnToggleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UnremovaeableOnToggleComponent, BeingUnequippedAttemptEvent>(OnBeingUnequipped);
    }

    private void OnBeingUnequipped(Entity<UnremovaeableOnToggleComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (!TryComp<ItemToggleComponent>(ent, out var itemToggle)) return;
        if (itemToggle.Activated)
        {
            args.Cancel();
            args.Reason = "unremoveable-on-toggle-fail";
        }
    }
}
