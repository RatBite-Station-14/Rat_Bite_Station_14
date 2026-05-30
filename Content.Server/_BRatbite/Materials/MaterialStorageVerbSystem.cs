using Content.Shared.Materials;
using Content.Server.Materials;
using Content.Shared.Verbs;

namespace Content.Server._BRatbite.Materials;

/// <summary>
/// Add eject verb to material systems
/// </summary>
public sealed class MaterialStorageVerbSystem : EntitySystem
{
    [Dependency] private MaterialStorageSystem _storageSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MaterialStorageComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
    }

    private void AddAlternativeVerbs(Entity<MaterialStorageComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !ent.Comp.AddEjectVerb || !ent.Comp.CanEjectStoredMaterials)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("container-verb-text-empty"),
            Category = VerbCategory.Eject,
            Priority = 1,
            Act = () =>
            {
                _storageSystem.EjectAllMaterial(ent.Owner);
            }
        });
    }
}
