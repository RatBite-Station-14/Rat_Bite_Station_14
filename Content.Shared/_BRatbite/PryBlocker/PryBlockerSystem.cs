using Content.Shared.Tools.Systems;

namespace Content.Shared._BRatbite.PryBlocker;

/// <summary>
/// This handles...
/// </summary>
public sealed class PryBlockerSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PryBlockerComponent, ToolPryAttemptEvent>(OnToolPryAttempt);
    }

    private void OnToolPryAttempt(Entity<PryBlockerComponent> ent, ref ToolPryAttemptEvent args) =>
        args.Cancel();
}
