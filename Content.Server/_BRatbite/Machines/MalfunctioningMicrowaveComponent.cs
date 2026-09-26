using Content.Server.Kitchen.Components;

namespace Content.Server._BRatbite.Machines;

/// <summary>
/// Placed on malfunctioning microwaves. Taken from <see cref="MicrowaveComponent"/>
/// <seealso cref="MalfunctioningMicrowaveSystem"/>
/// </summary>
[RegisterComponent]
public sealed partial class MalfunctioningMicrowaveComponent : Component
{
    [DataField]
    public TimeSpan NextMalfunction = TimeSpan.Zero;

    /// <summary>
    /// How frequently the microwave can malfunction.
    /// </summary>
    [DataField]
    public float MalfunctionInterval = 1.0f;

    /// <summary>
    /// Chance of an explosion occurring when we microwave a metallic object
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ExplosionChance = .1f;

    /// <summary>
    /// Chance of lightning occurring when we microwave a metallic object
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float LightningChance = .75f;

    /// <summary>
    /// If this microwave can give ids accesses without exploding
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool CanMicrowaveIdsSafely = true;
}
