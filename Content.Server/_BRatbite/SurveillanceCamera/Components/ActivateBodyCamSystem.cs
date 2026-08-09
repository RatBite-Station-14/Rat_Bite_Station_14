using Content.Goobstation.Shared.Clothing.Systems;
using Content.Server.SurveillanceCamera;
using Content.Shared.Chat;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory.Events;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Content.Shared.Verbs;

namespace Content.Server._BRatbite.SurveillanceCamera.Components;

public sealed partial class ActivateBodyCamSystem : EntitySystem
{
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly SurveillanceCameraSystem _surveillanceCameraSystem = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyCamComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<BodyCamComponent, GotEquippedEvent>(OnEquip, after: [typeof(ClothingGrantingSystem)]);
        SubscribeLocalEvent<BodyCamComponent, GotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<BodyCamComponent, ExaminedEvent>(OnExamine);
    }

    private void AddAlternativeVerbs(Entity<BodyCamComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !TryComp<SurveillanceCameraComponent>(ent, out var cameraComp))
            return;
        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(cameraComp.Active ? "body-cam-turn-off" : "body-cam-turn-on"),
            Act = () => ToggleBodyCam((ent, ent.Comp, cameraComp), user),
        });
    }

    private void ToggleBodyCam(Entity<BodyCamComponent, SurveillanceCameraComponent> ent, EntityUid user)
    {
        _chatSystem.TrySendInGameICMessage(
            user,
            Loc.GetString("body-cam-action-turn-" + (ent.Comp2.Active ? "off" : "on")),
            InGameICChatType.Emote, false);
        _surveillanceCameraSystem.SetActive(ent.Owner, !ent.Comp2.Active, ent.Comp2);
        UpdateStationAiVision(ent);
    }

    private void UpdateStationAiVision(Entity<BodyCamComponent> ent)
    {
        if (ent.Comp.Equipee is not { } owner || !TryComp<SurveillanceCameraComponent>(ent, out var cameraComp)) return;
        if (TryComp<StationAiVisionComponent>(owner, out var stationVision))
            _stationAiSystem.SetVisionEnabled((owner, stationVision), cameraComp.Active);
    }

    private void OnEquip(Entity<BodyCamComponent> ent, ref GotEquippedEvent args)
    {
        ent.Comp.Equipee = args.Equipee;
        UpdateStationAiVision(ent);
    }

    private void OnUnequip(Entity<BodyCamComponent> ent, ref GotUnequippedEvent args)
    {
        ent.Comp.Equipee = null;
        // No need to update station AI vision here, the component got
        // removed from the equipee
    }


    private void OnExamine(Entity<BodyCamComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<SurveillanceCameraComponent>(ent, out var cameraComp)) return;
        args.PushMarkup(Loc.GetString(cameraComp.Active ? "body-cam-examine-active" : "body-cam-examine-inactive"));
    }
}
