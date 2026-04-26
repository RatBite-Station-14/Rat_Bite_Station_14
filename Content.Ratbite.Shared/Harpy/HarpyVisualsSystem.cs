// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;

namespace Content.Ratbite.Shared.Harpy;

public sealed class HarpyVisualsSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string HarpyWingsTag = "HidesHarpyWings";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HarpySingerComponent, DidEquipEvent>(OnDidEquipEvent);
        SubscribeLocalEvent<HarpySingerComponent, DidUnequipEvent>(OnDidUnequipEvent);
    }

    private void OnDidEquipEvent(EntityUid uid, HarpySingerComponent component, DidEquipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            _appearance.SetData(uid, HumanoidVisualLayers.RArm, false);
            _appearance.SetData(uid, HumanoidVisualLayers.Tail, false);
        }
    }

    private void OnDidUnequipEvent(EntityUid uid, HarpySingerComponent component, DidUnequipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, HarpyWingsTag))
        {
            _appearance.SetData(uid, HumanoidVisualLayers.RArm, true);
            _appearance.SetData(uid, HumanoidVisualLayers.Tail, true);
        }
    }
}
