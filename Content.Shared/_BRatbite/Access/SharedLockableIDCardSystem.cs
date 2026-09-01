using Content.Shared.Lock;

namespace Content.Shared._BRatbite.Access;

public abstract partial class SharedLockableIDCardSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LockableIDCardComponent, LockToggleAttemptEvent>(OnLockToggleAttempt);
    }


    private void OnLockToggleAttempt(Entity<LockableIDCardComponent> ent, ref LockToggleAttemptEvent args)
    {
        var lockComp = EnsureComp<LockComponent>(ent);
        // Use our custom logic to unlock
        args.Cancelled = lockComp.Locked;
    }
}
