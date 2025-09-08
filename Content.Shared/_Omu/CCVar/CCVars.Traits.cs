using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     The maximum number of traits that the player is allowed to select in the profile editor.
    ///     If this value is negative, then players are allowed to select as many traits as they wish.
    /// </summary>
    public static readonly CVarDef<int>
        TraitsMaxTraits = CVarDef.Create("traits.max_traits", 14, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Whether "global" trait points are a thing.
    /// </summary>
    public static readonly CVarDef<bool>
        TraitsGlobalPointsEnabled = CVarDef.Create("traits.global_points_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     The default number of trait points given to each character.
    /// </summary>
    public static readonly CVarDef<int>
        TraitsDefaultPoints = CVarDef.Create("traits.default_points", 10, CVar.SERVER | CVar.REPLICATED);

}
