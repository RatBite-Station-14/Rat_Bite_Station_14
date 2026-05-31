using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class AppendageToggleClothingSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AppendageToggleClothingComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<AppendageToggleClothingComponent, ClothingGotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<AppendageToggleClothingComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);
    }

    private void OnEquipped(Entity<AppendageToggleClothingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ApplyVisibility(ent, args.Wearer, hideAppendages: ent.Comp.HideAppendages);
    }

    private void OnUnequipped(Entity<AppendageToggleClothingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ApplyVisibility(ent, args.Wearer, hideAppendages: false);

        if (!ent.Comp.HideAppendages)
            return;

        ent.Comp.HideAppendages = false;
        Dirty(ent);
    }

    private void OnVerb(Entity<AppendageToggleClothingComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!TryComp(ent, out ClothingComponent? clothing) ||
            clothing.InSlot == null ||
            !IsValidSlot(ent.Comp, clothing))
        {
            return;
        }

        var wearer = Transform(ent).ParentUid;
        if (!HasComp<HumanoidAppearanceComponent>(wearer))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Act = () =>
            {
                ent.Comp.HideAppendages = !ent.Comp.HideAppendages;
                Dirty(ent);
                ApplyVisibility(ent, wearer, ent.Comp.HideAppendages);
            },
            Text = Loc.GetString(ent.Comp.HideAppendages ? ent.Comp.ShowMessage : ent.Comp.HideMessage),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
            Priority = 1,
        });
    }

    private void ApplyVisibility(Entity<AppendageToggleClothingComponent> ent, Entity<HumanoidAppearanceComponent?> wearer, bool hideAppendages)
    {
        if (_timing.ApplyingState)
            return;

        if (!TryComp(ent, out ClothingComponent? clothing) ||
            !Resolve(wearer.Owner, ref wearer.Comp, false) ||
            !IsValidSlot(ent.Comp, clothing) ||
            clothing.InSlotFlag is not { } slot ||
            slot == SlotFlags.NONE)
        {
            return;
        }

        var dirty = false;
        foreach (var layer in ent.Comp.Layers)
        {
            _humanoid.SetLayerVisibility(wearer!, layer, !hideAppendages, slot, ref dirty);
        }

        if (dirty)
            Dirty(wearer!);
    }

    private static bool IsValidSlot(AppendageToggleClothingComponent component, ClothingComponent clothing)
    {
        if (clothing.InSlotFlag is not { } slot || slot == SlotFlags.NONE)
            return false;

        return component.Slots == SlotFlags.NONE
            ? clothing.Slots.HasFlag(slot)
            : component.Slots.HasFlag(slot);
    }
}
