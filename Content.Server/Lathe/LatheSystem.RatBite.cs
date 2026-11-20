using System.Linq;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Robust.Shared.Prototypes;

namespace Content.Server.Lathe;

public sealed partial class LatheSystem
{
    /// <summary>
    /// Tries to output result of lathe recipe <paramref name="resultProto"/> into this lathe's material storage.
    /// Amount inserted is multiplied by <paramref name="count"/>
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="resultProto"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool TryOutputToStorage(Entity<LatheComponent> ent, EntProtoId resultProto, int count = 1)
    {
        var prototype = _proto.Index(resultProto);

        if (!ent.Comp.OutputToStorage)
            return false;

        if (!prototype.TryGetComponent<PhysicalCompositionComponent>(out var composition, _factory))
            return false;

        var multipliedMaterials = composition.MaterialComposition.Select(pair => (pair.Key, pair.Value * count))
            .ToDictionary();
        return _materialStorage.TryChangeMaterialAmount(ent.Owner, multipliedMaterials);
    }
}
