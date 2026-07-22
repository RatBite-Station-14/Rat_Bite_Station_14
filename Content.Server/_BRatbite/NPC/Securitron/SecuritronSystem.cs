using Content.Server.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Emag.Components;
using Content.Shared.Humanoid;
using Content.Shared.Security.Components;
using Content.Shared.Security;
using Content.Server.IdentityManagement;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.PDA;
using System.Linq;
using Content.Server.Access.Components;
using Content.Shared.Contraband;
using Content.Shared.Inventory;
using Content.Shared.Emag.Systems;


namespace Content.Server._BRatbite.NPC.Securitron;

public sealed partial class SecuritronSystem : EntitySystem
{
    [Dependency] private readonly CuffableSystem _cuffableSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly IdentitySystem _identitySystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly ContrabandSystem _contrabandSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SecuritronComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SecuritronComponent, GetMeleeWeaponEvent>(OnGetWeapon);
        SubscribeLocalEvent<SecuritronComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnGetWeapon(Entity<SecuritronComponent> ent, ref GetMeleeWeaponEvent args)
    {
        args.Handled = true;
        args.Weapon = ent.Comp.Stunbaton;
    }

    private void OnMapInit(Entity<SecuritronComponent> ent, ref MapInitEvent args)
    {
        // TODO: put it in the inventory
        // We're leaking stunbatons
        ent.Comp.Stunbaton = Spawn(ent.Comp.WeaponPrototype);
    }

    public bool TryCuffAndPull(Entity<SecuritronComponent?> ent, EntityUid target)
    {
        if (!Resolve(ent.Owner, ref ent.Comp)) return false;
        if (!_interactionSystem.InRangeUnobstructed(ent.Owner, target)) return false;
        if (!TryComp<CuffableComponent>(target, out var cuffable)) return false;
        if (_cuffableSystem.IsCuffed((target, cuffable))) return true;
        var cuffs = SpawnAtPosition(ent.Comp.CuffsPrototype, Transform(ent).Coordinates);
        if (!_cuffableSystem.TryAddNewCuffs(target, ent, cuffs, component: cuffable))
        {
            Del(cuffs);
            return false;
        }
        return _pullingSystem.TryStartPull(ent, target);
    }

    public int GetTargetThreatLevel(Entity<SecuritronComponent> ent, EntityUid uid)
    {
        if (HasComp<EmaggedComponent>(ent)) return 10;
        var score = 0;
        var humanoid = CompOrNull<HumanoidAppearanceComponent>(uid);
        if (humanoid?.Species != ent.Comp.BiasSpecies) score += ent.Comp.BiasSpeciesThreatLevel;
        score += GetWantedThreatLevel(ent, uid);
        score += GetJobThreatLevel(ent, uid);
        score += GetContrabandThreatLevel(ent, uid);
        if (uid == ent.Comp.AttackerEntity) score += ent.Comp.AttackedThreatLevel;
        return score;
    }

    private int GetWantedThreatLevel(Entity<SecuritronComponent> ent, EntityUid uid)
    {
        var criminalRecord = CompOrNull<CriminalRecordComponent>(uid);
        if (!ent.Comp.WantedThreatLevels.TryGetValue(criminalRecord?.SecurityStatus ?? SecurityStatus.None, out var value)) return 0;

        return value;
    }

    private int GetJobThreatLevel(Entity<SecuritronComponent> ent, EntityUid uid)
    {
        if (GetIdCardOrNull(uid) is not { } idCard) return ent.Comp.UnknownThreatLevel;
        if (HasComp<AgentIDCardComponent>(idCard)) return ent.Comp.AgentIdThreatLevel;
        return idCard.Comp.JobDepartments.Sum(department => ent.Comp.DepartmentThreatLevels.GetValueOrDefault(department));
    }

    private int GetContrabandThreatLevel(Entity<SecuritronComponent> ent, EntityUid uid)
    {
        // Return 0 when no ID card, so people don't randomly get
        // arrested when they remove id card
        if (GetIdCardOrNull(uid) is null) return 0;
        var slotsToCheck = _inventorySystem.GetSlotEnumerator(uid, ent.Comp.SlotsToCheck);
        var threatLevel = 0;
        while (slotsToCheck.NextItem(out var item))
        {
            if (!TryComp<ContrabandComponent>(item, out var contraband)) continue;
            if (!_contrabandSystem.CanWearContraband((item, contraband), uid))
                threatLevel += ent.Comp.ContrabandThreatLevel;
        }
        return threatLevel;
    }

    private Entity<IdCardComponent>? GetIdCardOrNull(EntityUid uid)
    {
        if (_accessReaderSystem.FindAccessItemsInventory(uid, out var items))
        {
            foreach (var item in items)
            {
                if (TryComp<IdCardComponent>(item, out var id))
                    return (item, id);

                if (TryComp<PdaComponent>(item, out var pda)
                    && pda.ContainedId != null
                    && TryComp(pda.ContainedId, out id)
                )
                    return (pda.ContainedId.Value, id);
            }
        }
        return null;
    }

    private void OnEmagged(Entity<SecuritronComponent> ent, ref GotEmaggedEvent args)
    {
        if ((args.Type & EmagType.Interaction) != 0)
            args.Handled = true;
    }
}
