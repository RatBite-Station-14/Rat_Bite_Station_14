namespace Content.Shared._BRatbite.PryBlocker;

/// <summary>
/// This is used for preventing tiles from being pried beneath an anchored entity with this component.
/// Primarily for preventing lattice beneath field generators being cut.
/// </summary>
[RegisterComponent]
public sealed partial class PryBlockerComponent : Component;
