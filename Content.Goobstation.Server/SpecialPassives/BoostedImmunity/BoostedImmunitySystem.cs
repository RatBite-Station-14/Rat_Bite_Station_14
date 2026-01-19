using Content.Goobstation.Shared.Disease.Systems;
using Content.Goobstation.Shared.Disease.Components;
using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;





    protected override void RemoveDiseases(Entity<BoostedImmunityComponent> ent)
    {
        _disease.TryCureAll(ent.Owner);
    }
}
