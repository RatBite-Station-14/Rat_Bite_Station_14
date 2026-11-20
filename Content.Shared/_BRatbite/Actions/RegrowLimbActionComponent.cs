using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Actions;

[RegisterComponent]
public sealed partial class RegrowLimbActionComponent : Component
{
    [DataField]
    public float RequiredHunger = 150f;

    [DataField]
    public EntProtoId Action = "ActionRegrowLimb";

    [ViewVariables]
    public EntityUid? ActionEntity;
}

public sealed partial class RegrowLimbActionEvent : InstantActionEvent;
