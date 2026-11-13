using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.TrackingHud.MarkerMonitor;

[Prototype]
public sealed partial class MarkerPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField]
    // If true, it will not show up on the HoS' monitor
    public bool HideFromMonitor = false;
}
