using Content.Shared.Hands;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;

namespace Content.Shared._BRatbite.Interact;

public sealed partial class AddComponentsOnInteractSystem : EntitySystem
{
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AddComponentsOnInteractComponent, UseInHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(Entity<AddComponentsOnInteractComponent> ent, ref UseInHandEvent args)
    {
        _entityManager.AddComponents(args.User, ent.Comp.Components);
        if (ent.Comp.PopupText != null)
            _popupSystem.PopupClient(Loc.GetString(ent.Comp.PopupText), args.User, args.User);
    }
}
