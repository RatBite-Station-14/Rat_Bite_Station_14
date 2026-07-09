using Content.Shared.Access.Systems;

namespace Content.Client._BRatbite.Silicons.StationAi;

public sealed partial class TrackJobChangeSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        UpdatesOutsidePrediction = true;
    }

    public override void Update(float _)
    {
        // This is a client side only check, so it's not too expensive
        // because it will only do it to entities in the PVS
        var enumerator = EntityQueryEnumerator<TrackJobChangeComponent>();
        while (enumerator.MoveNext(out var uid, out var jobChangeComp))
        {
            _accessReaderSystem.GetIdCardComponent(uid, out var idCardComponent);

            if (jobChangeComp.LastJobIcon != idCardComponent?.JobIcon)
            {
                jobChangeComp.LastJobIcon = idCardComponent?.JobIcon;
                var ev = new JobChangeEvent(jobChangeComp.LastJobIcon);
                RaiseLocalEvent(uid, ref ev);
            }
        }
    }
}
