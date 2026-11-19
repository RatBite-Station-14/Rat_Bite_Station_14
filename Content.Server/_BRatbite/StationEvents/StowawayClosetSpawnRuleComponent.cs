// This creative work/file is freely and wholly released into the public domain by its original owner/creator, Fishied, under CC0 1.0: https://creativecommons.org/publicdomain/zero/1.0

using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.StationEvents;

// spawns a mob into a random maints locker
[RegisterComponent]
[Access(typeof(StowawayClosetSpawnRule))]
public sealed partial class StowawayClosetSpawnRuleComponent : Component
{
    [DataField(required: true)] public EntProtoId MobToSpawn;
}
