using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Prototypes;
using Content.Shared.Body.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.Actions;

public sealed partial class RegrowLimbActionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly HungerSystem _hungerSystem = default!;
    [Dependency] private readonly TraumaSystem _traumaSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RegrowLimbActionComponent, RegrowLimbActionEvent>(OnRegrowLimb);
        SubscribeLocalEvent<RegrowLimbActionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnRegrowLimb(Entity<RegrowLimbActionComponent> ent, ref RegrowLimbActionEvent args)
    {
        if (!TryComp<BodyComponent>(ent, out var body) || body.Prototype is not { } || !TryComp<HungerComponent>(ent, out var hunger)) return;
        if (_hungerSystem.GetHunger(hunger) < ent.Comp.RequiredHunger)
        {
            _popup.PopupClient(Loc.GetString("regrow-limb-action-food"), ent, ent);
            return;
        }

        if (!_bodySystem.TryGetRootPart(ent.Owner, out var rootPart)) return;

        if (_net.IsClient) return;

        var bodyPrototype = _proto.Index(body.Prototype);
        var bodyParts = new List<Entity<BodyPartComponent>>() { rootPart.Value };
        while (bodyParts.Any())
        {
            var part = bodyParts.Pop();
            foreach (var child in part.Comp.Children)
            {
                if (!_containerSystem.TryGetContainer(part.Owner, SharedBodySystem.GetPartSlotContainerId(child.Value.Id), out var baseContainer) || baseContainer.ContainedEntities.Count == 0)
                {
                    if (!bodyPrototype.Slots.TryGetValue(child.Key, out var bodyPartProto)) continue;
                    if (!_bodySystem.CreateBodyPart(part, bodyPartProto, child.Key, out var newPart)) continue;
                    _hungerSystem.ModifyHunger(ent.Owner, -ent.Comp.RequiredHunger, hunger);
                    if (_traumaSystem.TryGetWoundableTrauma(part.Owner, out var traumas, "Dismemberment"))
                    {
                        var newPartComp = Comp<BodyPartComponent>(newPart.Value);
                        foreach (var trauma in traumas.FindAll((trauma) => trauma.Comp.TargetType == (newPartComp.PartType, newPartComp.Symmetry)))
                        {
                            _traumaSystem.RemoveTrauma(trauma);
                        }
                    }
                    _popup.PopupEntity(Loc.GetString("regrow-limb-success", ("name", Name(newPart.Value))), ent, ent);

                    return;
                }
                var childEnt = baseContainer.ContainedEntities[0];
                bodyParts.Add((childEnt, Comp<BodyPartComponent>(childEnt)));
            }
        }
    }

    private void OnMapInit(Entity<RegrowLimbActionComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }
}
