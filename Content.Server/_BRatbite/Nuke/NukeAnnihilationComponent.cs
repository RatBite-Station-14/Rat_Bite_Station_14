namespace Content.Server._BRatbite.Nuke;

/// <summary>
/// Marks an entity as an expanding "delete bubble" spawned at a nuke's epicenter, similar to a singularity's
/// event horizon. Everything caught inside the (growing) radius is deleted outright, ahead of the slower
/// explosion tile/damage falloff that follows behind it.
/// </summary>
[RegisterComponent]
[Access(typeof(NukeAnnihilationSystem))]
public sealed partial class NukeAnnihilationComponent : Component
{
    /// <summary>
    /// Current radius of the deletion bubble, in tiles.
    /// </summary>
    [DataField]
    public float CurrentRadius;

    /// <summary>
    /// Radius the bubble stops expanding at. Once reached, the bubble entity deletes itself.
    /// </summary>
    [DataField]
    public float MaxRadius = 20f;

    /// <summary>
    /// How many tiles per second the bubble's radius grows.
    /// </summary>
    [DataField]
    public float ExpansionRate = 6f;

    /// <summary>
    /// How often the bubble scans for entities to delete. Spreading this out over multiple ticks (rather than
    /// doing it every frame) avoids a lag spike from deleting a huge amount of entities all at once.
    /// </summary>
    [DataField]
    public TimeSpan ExpandInterval = TimeSpan.FromSeconds(0.2);

    /// <summary>
    /// Next time the bubble is allowed to expand/consume.
    /// </summary>
    [DataField]
    public TimeSpan NextExpandTime;
}
