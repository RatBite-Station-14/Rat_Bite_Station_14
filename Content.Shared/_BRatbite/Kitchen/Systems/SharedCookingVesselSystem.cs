using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Tools.Components;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Shared._BRatbite.Kitchen.Systems;

/// <summary>
/// Handles <see cref="CookingVesselComponent"/>
/// </summary>
public abstract class SharedCookingVesselSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _sharedPowerReceiverSystem = default!;
    [Dependency] private readonly SharedPopupSystem _sharedPopupSystem = default!;
    [Dependency] private readonly SharedItemSystem _sharedItemSystem = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CookingVesselComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CookingVesselComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CookingVesselComponent, AnchorStateChangedEvent>(OnAnchorChanged);

        SubscribeLocalEvent<CookingVesselComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<CookingVesselComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || HasComp<ActiveCookingVesselComponent>(ent))
            return;

        args.Verbs.Add(
            new AlternativeVerb
            {
                Text = Loc.GetString("cooking-vessel-component-empty-all-verb-text"),
                Act = () =>
                {
                    _container.EmptyContainer(ent.Comp.Storage);
                },
                Priority = 4,
            }
        );

        var user = args.User;
        if (!HasComp<ItemComponent>(args.Using) || args.Using is not { } item) // necessary for null type checking
            return;
        args.Verbs.Add(
            new AlternativeVerb
            {
                Text = Loc.GetString("cooking-vessel-component-insert-item-verb-text"),
                Act = () =>
                {
                    _sharedHandsSystem.TryDropIntoContainer(user, item, ent.Comp.Storage);
                },
                Priority = 4,
            }
        );
    }

    private void OnAnchorChanged(Entity<CookingVesselComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored || TerminatingOrDeleted(ent))
            return;
        _container.EmptyContainer(ent.Comp.Storage);
    }

    private void OnInteractUsing(Entity<CookingVesselComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;
        if (ent.Comp.RequiresPower)
        {
            if (!_sharedPowerReceiverSystem.IsPowered((ent, null)))
            {
                _sharedPopupSystem.PopupPredicted(Loc.GetString("cooking-vessel-component-interact-using-no-power"),
                    ent,
                    args.User);
                return;
            }
        }

        if (HasComp<ToolComponent>(args.Used) || HasComp<ContainerCookerComponent>(args.Used) || HasComp<DrainableSolutionComponent>(args.Used)) // they'll want to insert it via right-click verb.
            return;

        if (TryComp<ItemComponent>(args.Used, out var item))
        {
            if (_sharedItemSystem.GetSizePrototype(item.Size) >
                _sharedItemSystem.GetSizePrototype(ent.Comp.MaxItemSize))
            {
                _sharedPopupSystem.PopupPredicted(Loc.GetString("cooking-vessel-component-interact-item-too-big"),
                    ent,
                    args.User);
                return;
            }
        }
        else
        {
            _sharedPopupSystem.PopupPredicted(Loc.GetString("cooking-vessel-component-interact-using-fail"),
                ent,
                args.User);
        }

        if (ent.Comp.Storage.Count >= ent.Comp.ItemCapacity)
        {
            _sharedPopupSystem.PopupPredicted(Loc.GetString("cooking-vessel-component-interact-at-capacity"),
                ent,
                args.User);
            return;
        }

        args.Handled = true;
        _sharedHandsSystem.TryDropIntoContainer(args.User, args.Used, ent.Comp.Storage);
    }

    public void EjectEntity(Entity<CookingVesselComponent?> ent, EntityUid toEject)
    {
        if (!GetContents(ent, out var cookingVesselComponent))
            return;
        _container.Remove(toEject, cookingVesselComponent.Storage);
    }

    public void EjectAllContents(Entity<CookingVesselComponent?> ent)
    {
        if (!GetContents(ent.Owner, out var cookingVesselComponent))
            return;
        _container.EmptyContainer(cookingVesselComponent.Storage);
    }

    protected bool GetContents(EntityUid ent, [NotNullWhen(true)] out CookingVesselComponent? component)
    {
        component = null;
        if (!TryComp<CookingVesselComponent>(ent, out var definiteComponent))
            return false;
        component = definiteComponent;
        return component.Storage.ContainedEntities.Any();
    }

    private void OnInit(Entity<CookingVesselComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Storage = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
    }
}
