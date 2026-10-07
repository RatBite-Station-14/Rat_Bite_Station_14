namespace Content.Server._BRatbite.GameTicking.Rules;

[RegisterComponent, Access(typeof(SusRatThreatSystem))]
public sealed partial class SusRatThreatComponent : Component
{
    [DataField]
    public TimeSpan MidroundDelay = TimeSpan.FromHours(2);

    [DataField]
    public float MidroundVariance = 0.1f;

    [DataField]
    public TimeSpan RoundEnderDelay = TimeSpan.FromHours(5);

    [DataField]
    public float RoundEnderVariance = 0.15f;

    [ViewVariables]
    public TimeSpan MidroundTime;

    [ViewVariables]
    public TimeSpan RoundEnderTime;

    [ViewVariables]
    public bool RoundEnderRolled;
}
