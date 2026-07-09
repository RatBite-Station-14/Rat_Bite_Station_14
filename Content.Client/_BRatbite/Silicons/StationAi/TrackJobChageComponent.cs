using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Client._BRatbite.Silicons.StationAi;

[RegisterComponent]
public sealed partial class TrackJobChangeComponent : Component
{
    public ProtoId<JobIconPrototype>? LastJobIcon = null;
}


// Raised on an entity with TrackJobChangeComponent when the job changes
[ByRefEvent]
public record struct JobChangeEvent(ProtoId<JobIconPrototype>? NewJob);
