namespace Content.Shared._BRatbite.Strip;

[RegisterComponent]
public sealed partial class StrippableWarningComponent : Component
{
    [DataField]
    public EntityUid? LastEntity;

    [DataField]
    public bool HasBeenWarned = false;
}
