using Robust.Shared.Network;

namespace Content.Server._BRatbite.PermaBrig.NTRTermination;

[RegisterComponent]
// Entities with this component are able to fill and send termination requests
public sealed partial class NTRTerminationComponent : Component;

[RegisterComponent]
public sealed partial class NTRTerminationPaperComponent : Component
{
    [DataField]
    public TimeSpan AddedTime = TimeSpan.FromMinutes(60);

    [DataField]
    public NetUserId? Terminator;

    [DataField]
    public NetUserId? TerminatedUser;

    [DataField]
    public List<string> AcceptedStamps = new List<string> { "stamp-component-stamped-name-captain", "stamp-component-stamped-name-hop" };

    [DataField]
    // If not null, this has been forged
    public NetUserId? ForgedBy;

    [DataField]
    public LocId AcceptedMessage = "ntr-termination-accept-message";

    [DataField]
    public LocId ForgedMessage = "ntr-termination-forged-message";

    [DataField]
    public LocId AcceptedStampName = "ntr-termination-accept-stamp";

    [DataField]
    public Color AcceptedStampColor = new(0x00, 0x66, 0x00); // same as centcom
}

[RegisterComponent]
// Component given to all command roles, indicating that they can be terminated
public sealed partial class NTRTerminatableComponent : Component
{
    [DataField]
    public NetUserId? LastMind; // Indicates last mind of this user so
                                // that if the player ghosts/suicides
                                // they can still be terminated
}
