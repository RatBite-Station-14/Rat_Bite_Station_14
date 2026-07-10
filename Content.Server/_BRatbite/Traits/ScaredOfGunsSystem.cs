using Content.Shared.Popups;
using Robust.Shared.Random;
using Content.Shared.Hands.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;

namespace Content.Server._BRatbite.Traits;

public sealed partial class ScaredOfGunsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScaredOfGunsComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<ScaredOfGunsComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_random.Prob(ent.Comp.DropGunChance) && TryComp<HandsComponent>(ent.Owner, out var handComp))
        {
            if (_handsSystem.TryDrop(new(ent.Owner, handComp)))
            {
                _popupSystem.PopupEntity(Loc.GetString("gun-fail-drop-shoot"), ent.Owner, ent.Owner);
                args.Cancel();
                return;
            }

        }
        if (_random.Prob(ent.Comp.MissShotChance))
        {
            _popupSystem.PopupEntity(Loc.GetString("gun-fail-shoot"), ent.Owner, ent.Owner);
            args.Cancel();
            return;
        }
    }

}
