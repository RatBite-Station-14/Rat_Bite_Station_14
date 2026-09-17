namespace Content.Server._BRatbite.CaptainIDGib;

[RegisterComponent]
public sealed partial class CaptainIDGibComponent : Component
{
    [DataField(required: true)]
    public HashSet<string> TargetUserIds = [];
}
