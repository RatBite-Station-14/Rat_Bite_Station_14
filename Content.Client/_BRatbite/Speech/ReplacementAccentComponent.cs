namespace Content.Client._BRatbite.Speech;

[RegisterComponent]
public sealed partial class ReplacementAccentComponent : Component
{
    [DataField(required: true)]
    public string Accent = default!;
}
