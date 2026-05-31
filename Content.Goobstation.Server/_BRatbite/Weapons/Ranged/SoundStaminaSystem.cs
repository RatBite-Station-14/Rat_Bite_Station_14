using System.Linq;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Popups;
using Content.Goobstation.Common.Traits;
using Content.Shared.Inventory;
using Content.Goobstation.Server.Flashbang;
using Content.Shared._BRatbite.Weapons.Ranged;
using Content.Shared.Weapons.Ranged;

namespace Content.Goobstation.Server._BRatbite.Weapons.Ranged;

// Applies stamina damage when shooting loud weapons
public sealed partial class SoundStaminaSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LoudGunComponent, GunShotEvent>(OnGunShot);
    }

    private void OnGunShot(Entity<LoudGunComponent> ent, ref GunShotEvent args)
    {
        // Deaf people don't hear guns
        if (HasComp<DeafComponent>(args.User))
            return;

        // They are wearing hear protection
        if (_inventorySystem.GetHandOrInventoryEntities(args.User, SlotFlags.EARS).Any(e => HasComp<FlashSoundSuppressionComponent>(e)))
            return;


        _staminaSystem.TakeStaminaDamage(args.User, ent.Comp.StaminaDamage, logDamage: false);
    }
}
