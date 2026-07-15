using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;

namespace Content.Shared._BRatbite.Traits;

public sealed partial class NudistSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NudistComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<NudistComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NudistComponent, DidEquipEvent>(OnRecomputeNeeded);
        SubscribeLocalEvent<NudistComponent, DidUnequipEvent>(OnRecomputeNeeded);
    }

    private void OnRefreshMovementSpeed(Entity<NudistComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.CachedModifier.SpeedModifier);
    }

    private void RecomputeModifiers(Entity<NudistComponent> ent)
    {
        ent.Comp.CachedModifier = ent.Comp.BaseModifier;
        foreach (var (flags, modifier) in ent.Comp.ClothingModifier)
        {
            if (!_inventorySystem.TryGetContainerSlotEnumerator(ent.Owner, out var containerSlotEnum, flags)) continue;
            if (!containerSlotEnum.NextItem(out _)) continue;
            ent.Comp.CachedModifier.SpeedModifier *= modifier.SpeedModifier;
            ent.Comp.CachedModifier.StaminaModifier *= modifier.StaminaModifier;
        }
        Dirty(ent, ent.Comp);
        var stamMod = EnsureComp<StaminaModifierComponent>(ent);
        _staminaSystem.SetModifier(ent.Owner, ent.Comp.CachedModifier.StaminaModifier, null, stamMod);
    }

    private void OnMapInit(Entity<NudistComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<StaminaModifierComponent>(ent, out var staminaModifierComp))
            ent.Comp.BaseModifier.StaminaModifier *= staminaModifierComp.Modifier;
        RecomputeModifiers(ent);
    }

    private void OnRecomputeNeeded<T>(Entity<NudistComponent> ent, ref T _)
    {
        RecomputeModifiers(ent);
    }
}
