using Content.Shared.Lock;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Ratbite.Shared.FiringPin;

/// <summary>
/// This handles whether a weapon with a FiringPinComponent should be allowed to fire
/// </summary>
public sealed class FiringPinSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<FiringPinComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<FiringPinComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!TryComp<LockComponent>(ent, out var lockComponent))
            return;

        if (!lockComponent.Locked)
            return;

        if (HasComp<MindShieldComponent>(args.User))
            return;

        _popup.PopupClient(Loc.GetString("firing-pin-cant-fire"), ent, args.User);
        args.Cancel();
    }
}
