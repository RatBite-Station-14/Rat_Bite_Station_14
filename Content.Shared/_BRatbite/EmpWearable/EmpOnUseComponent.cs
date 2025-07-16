using Robust.Shared.Audio;

namespace Content.Shared._BRatbite.EmpGlove;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class EmpOnUseComponent : Component
{
    [DataField]
    public float EmpDrain = 5000f; //Most weapons have around 1000, IPCs depend on inserted battery with default battery being 750

    [DataField]
    public float EmpDuration = 10f;

    [DataField]
    public SoundSpecifier EmpSound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningbolt.ogg");
}
