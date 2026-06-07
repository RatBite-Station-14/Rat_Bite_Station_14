namespace Content.Shared.Roles;

public sealed partial class JobPrototype
{
    [DataField]
    public bool CanBeAntag { get; private set; } = true;

}
