using Content.Shared.Lock;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._BRatbite.FiringPin;

/// <summary>
/// This handles whether a weapon with a FiringPinComponent should be allowed to fire
/// </summary>
public sealed class FiringPinSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
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

        Popup.PopupClient(Loc.GetString("firing-pin-cant-fire"), ent, args.User);
        args.Cancel();
    }
}
