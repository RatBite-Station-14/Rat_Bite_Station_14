using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared._BRatbite.Item.ItemToggle;

public sealed partial class AccessItemToggleSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AccessReaderComponent, ItemToggleActivateAttemptEvent>((ent, ref args) => OnToggleAttempt(ent, args.User, ref args.Cancelled, ref args.Popup));
        SubscribeLocalEvent<AccessReaderComponent, ItemToggleDeactivateAttemptEvent>((ent, ref args) => OnToggleAttempt(ent, args.User, ref args.Cancelled, ref args.Popup));
    }

    private void OnToggleAttempt(Entity<AccessReaderComponent> ent, EntityUid? user, ref bool cancelled, ref string? popup)
    {
        if (user is { } u && _accessReaderSystem.IsAllowed(u, ent.Owner, ent.Comp)) return;
        cancelled = true;
        popup = Loc.GetString("lock-comp-has-user-access-fail");
    }
}
