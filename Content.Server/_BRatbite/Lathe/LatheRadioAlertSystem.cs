using Content.Server.Radio.EntitySystems;
using Content.Shared.Lathe;

namespace Content.Server._BRatbite.Lathe;

public sealed partial class LatheRadioAlertSystem : EntitySystem
{
    [Dependency] private readonly RadioSystem _radioSystem = default!;
    [Dependency] private readonly SharedLatheSystem _latheSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LatheComponent, LatheFinishPrintingEvent>(OnLathePrint);
    }

    private void OnLathePrint(Entity<LatheComponent> ent, ref LatheFinishPrintingEvent args)
    {
        if (args.Recipe.PrintMessageLocId is not { } printMessageLocId || args.Recipe.RadioChannel is not { } radioChannel || !TryComp<MetaDataComponent>(ent, out var metadata)) return;

        _radioSystem.SendRadioMessage(ent.Owner, Loc.GetString(printMessageLocId, [("recipeName", _latheSystem.GetRecipeName(args.Recipe)), ("fabName", metadata.EntityName)]), radioChannel, ent.Owner);
    }
}
