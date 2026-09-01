using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Access;

[RegisterComponent]
public sealed partial class LockableIDCardComponent : Component
{
    [ViewVariables]
    public string Password = "";
}
