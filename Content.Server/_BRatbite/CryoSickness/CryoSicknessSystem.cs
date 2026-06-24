using Content.Server.Antag;
using Content.Shared._BRatbite.CryoSickness;

namespace Content.Server._BRatbite.CryoSickness;

public sealed partial class CryoSicknessSystem : SharedCryoSicknessSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AfterAntagEntitySelectedEvent>(OnAntagSelected);
    }

    private void OnAntagSelected(ref AfterAntagEntitySelectedEvent args)
    {
        // This is mainly for thieves
        if (args.Def.Components.ContainsKey("Pacified") && TryComp<CryoSicknessComponent>(args.EntityUid, out var cryoSicknessComp))
        {
            cryoSicknessComp.HadPacifism = true;
        }
    }
}
