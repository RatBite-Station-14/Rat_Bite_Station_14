using Content.Server.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Emag.Components;


namespace Content.Server._BRatbite.NPC.Securitron;

public sealed partial class SecuritronSystem : EntitySystem
{
    [Dependency] private readonly CuffableSystem _cuffableSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SecuritronComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SecuritronComponent, GetMeleeWeaponEvent>(OnGetWeapon);
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
        return 10;
    }
}
