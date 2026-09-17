using Content.Server.Radio.EntitySystems;
using Content.Shared.Construction;
using Content.Shared.IdentityManagement;

namespace Content.Server._BRatbite.Construction.Completions;

[DataDefinition]
public sealed partial class Radio : IGraphAction
{
    [DataField(required: true)]
    public string RadioMessage;

    [DataField(required: true)]
    public string RadioChannel;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var radio = entityManager.System<RadioSystem>();
        radio.SendRadioMessage(
            uid,
            Loc.GetString(RadioMessage, ("entityName", Identity.Name(uid, entityManager))),
            RadioChannel,
            uid
        );
    }
}
