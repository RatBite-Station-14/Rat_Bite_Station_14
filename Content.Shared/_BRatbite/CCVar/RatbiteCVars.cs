using Robust.Shared.Configuration;

namespace Content.Shared._BRatbite.CCVar;

[CVarDefs]
public sealed partial class RatbiteCVars
{
    /// <summary>
    ///     Alt server to connect to (Default is ratbite 2)
    /// </summary>
    public static readonly CVarDef<string> AltServerIP =
        CVarDef.Create("misc.alt_server_ip", "136.243.32.120", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Alt server port to connect to (Default is ratbite 2)
    /// </summary>
    public static readonly CVarDef<int> AltServerPort =
        CVarDef.Create("misc.alt_server_port", 25503, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     How many seconds between requests
    /// </summary>
    public static readonly CVarDef<int> PopCountRefreshSeconds =
        CVarDef.Create("misc.pop_count_refresh_seconds", 60, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<string> AltServerName =
        CVarDef.Create("misc.alt_server_name", "Ratbite 2", CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> PointerScale = CVarDef.Create("accessibility.pointer_scale", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> PointerOutline = CVarDef.Create("accessibility.pointer_outline", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
