using Content.Shared.Interaction;

namespace Content.Shared._BRatbite.Mousetrap;

public abstract partial class SharedGiantMouseTrapSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GiantMouseTrapComponent, InteractHandEvent>(OnInteractHand);
    }

    protected virtual void OnInteractHand(Entity<GiantMouseTrapComponent> ent, ref InteractHandEvent args)
    {
        args.Handled = true;
    }
}
