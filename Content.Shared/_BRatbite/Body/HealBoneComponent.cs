namespace Content.Shared._BRatbite.Body;

[RegisterComponent]
public sealed partial class HealBoneComponent : Component
{
    [DataField]
    public float BoneHealingPerSecond = 1f;

    [DataField]
    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(0.5);
}
