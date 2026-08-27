using Content.Shared._BRatbite.Access;

namespace Content.Client._BRatbite.Access;

public sealed partial class EmergencyAccessSystem : SharedEmergencyAccessSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override bool IsAlertLevelReached(Entity<EmergencyAccessComponent> ent)
    {
        return ent.Comp.CurrentAlertLevel == ent.Comp.TargetAlert;
    }
}
