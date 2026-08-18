namespace Content.Server._BRatbite.Spawners;

[RegisterComponent]
public sealed partial class StowawaySpawnPointComponent : Component
{
    [DataField]
    public string ContainerId = "entity_storage";

    [DataField]
    public bool Secure = false;
}
