using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Administration;


[RegisterComponent, NetworkedComponent]
public sealed partial class RPHelpComponent : Component
{
}

public sealed partial class RPHelpActionEvent : InstantActionEvent;
