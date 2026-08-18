namespace Content.Shared.Tools.Systems;

/// <summary>
///     Sent when someone is attempting to pry a tile beneath an entity.
///     Every entity on the tile will be sent this event.
/// </summary>
public sealed class ToolPryAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid User;
    public readonly EntityUid Tool;

    public ToolPryAttemptEvent(EntityUid user, EntityUid tool)
    {
        User = user;
        Tool = tool;
    }
}
