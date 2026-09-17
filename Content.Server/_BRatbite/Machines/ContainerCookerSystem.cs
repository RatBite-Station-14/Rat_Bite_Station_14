using System.Linq;
using Content.Server.Chemistry.Components;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Server.Temperature.Systems;
using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Atmos;
using Content.Shared.Placeable;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._BRatbite.Machines;

/// <summary>
/// Handles containers that cook stuff.
/// <seealso cref="ContainerCookerComponent" />
/// </summary>
public sealed class ContainerCookerSystem : SharedContainerCookerSystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly CookingVesselSystem _cookingVesselSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveContainerCookerComponent, CookingVesselFinishedCooking>((ent, ref _) => StopCooking(ent));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var activeSolutionHeaters =
            EntityQueryEnumerator<SolutionHeaterComponent, ItemPlacerComponent, ActiveSolutionHeaterComponent>();
        while (activeSolutionHeaters.MoveNext(out var uid, out _, out var itemPlacerComponent, out _))
        {
            foreach (var placedEntity in itemPlacerComponent.PlacedEntities.Where(HasComp<ContainerCookerComponent>))
            {
                StartCooking(placedEntity, uid);
            }
        }

        var activeEntityHeaters = EntityQueryEnumerator<EntityHeaterComponent, ItemPlacerComponent>();
        while (activeEntityHeaters.MoveNext(out var uid, out var entityHeaterComponent, out var itemPlacerComponent))
        {
            if (!_powerReceiverSystem.IsPowered(uid) || entityHeaterComponent.Setting == EntityHeaterSetting.Off)
                continue;

            foreach (var placedEntity in itemPlacerComponent.PlacedEntities.Where(HasComp<ContainerCookerComponent>))
            {
                StartCooking(placedEntity, uid);
            }
        }

        var activeCookers = EntityQueryEnumerator<ActiveCookingVesselComponent, ActiveContainerCookerComponent>();
        while (activeCookers.MoveNext(out var uid, out _, out var activeContainerCookerComp))
        {
            if (!_powerReceiverSystem.IsPowered(activeContainerCookerComp.HeatSource) ||
                !TryComp<ItemPlacerComponent>(activeContainerCookerComp.HeatSource, out var itemPlacerComponent) ||
                !itemPlacerComponent.PlacedEntities.ToArray().Contains(uid) ||
                TryComp<EntityHeaterComponent>(activeContainerCookerComp.HeatSource, out var activeHeater) &&
                activeHeater.Setting == EntityHeaterSetting.Off)
                StopCooking(uid);
        }
    }

    private void StopCooking(EntityUid ent)
    {
        _temperatureSystem.ForceChangeTemperature(ent, Atmospherics.T20C); // reset heat tint
        RemCompDeferred<ActiveContainerCookerComponent>(ent);
        _cookingVesselSystem.StopCooking(ent);
    }

    private void StartCooking(EntityUid entity, EntityUid heatSource)
    {
        if (HasComp<ActiveCookingVesselComponent>(entity) ||
            !TryComp<ContainerCookerComponent>(entity, out var cookerComponent) ||
            HasComp<ActiveContainerCookerComponent>(entity))
            return;
        if (_cookingVesselSystem.StartCooking(entity, () => UpdateUserInterfaceState(entity)) is null)
            return;
        UpdateUserInterfaceState(entity);
        var activeContainerComponent = AddComp<ActiveContainerCookerComponent>(entity);
        activeContainerComponent.HeatSource = heatSource;
        _audioSystem.PlayPvs(cookerComponent.StartCookingSound, entity, AudioParams.Default);
        _popupSystem.PopupEntity(Loc.GetString("cooking-vessel-component-cooking-start"), entity);
    }
}
