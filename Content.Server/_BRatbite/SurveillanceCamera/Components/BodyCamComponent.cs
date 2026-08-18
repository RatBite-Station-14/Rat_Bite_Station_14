namespace Content.Server._BRatbite.SurveillanceCamera.Components;

[RegisterComponent]
public sealed partial class BodyCamComponent : Component
{
    [DataField]
    // Who is wearing the body cam
    public EntityUid? Equipee;
}
