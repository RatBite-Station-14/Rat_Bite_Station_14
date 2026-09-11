using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._BRatbite.Machines;

/// <inheritdoc/>
public sealed class TimedCookerSystem : SharedTimedCookerSystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _sharedPowerReceiverSystem = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _sharedAppearanceSystem = default!;
    [Dependency] private readonly CookingVesselSystem _cookingVesselSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimedCookerComponent, TimedCookerStartCookingMessage>(OnStartCookingMsg);
        SubscribeLocalEvent<TimedCookerComponent, CookingVesselFinishedCooking>(StopCooking);
    }

    private void StopCooking(Entity<TimedCookerComponent> ent, ref CookingVesselFinishedCooking args)
    {
        if (ent.Comp.PlayingStream is not null)
            ent.Comp.PlayingStream = _sharedAudioSystem.Stop(ent.Comp.PlayingStream);
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.Active, false);
        _sharedAudioSystem.PlayPvs(ent.Comp.FoodDoneSound, ent.Owner, AudioParams.Default);
        UpdateUserInterfaceState(ent);
    }

    private void OnStartCookingMsg(Entity<TimedCookerComponent> ent, ref TimedCookerStartCookingMessage args)
    {
        if (!_sharedPowerReceiverSystem.IsPowered(ent.Owner) || ent.Comp.CalculatedPresets is null)
            return;

        if (args.SelectedTimePreset > ent.Comp.CookTimePresets.Length + 1)
        {
            Log.Warning(
                $"Error when starting timed cooker ${ent.Owner}. Received invalid time preset from ${args.Actor}.");
            return;
        }
        // time preset 0 is instant, [1..N] is on the calculated time spans [0..N-1]
        var cookSeconds = args.SelectedTimePreset == 0
            ? 0
            : ent.Comp.CalculatedPresets[args.SelectedTimePreset - 1];
        var endCookingOn = _cookingVesselSystem.StartCooking(ent.Owner, () => UpdateUserInterfaceState(ent), args.Actor, cookSeconds);
        _sharedAudioSystem.PlayPvs(ent.Comp.StartCookingSound, ent, AudioParams.Default);
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.Active, true);
        UpdateUserInterfaceState(ent, endCookingOn);
        if (ent.Comp.LoopingSound is null)
            return;
        ent.Comp.PlayingStream = _sharedAudioSystem
            .PlayPvs(ent.Comp.LoopingSound, ent, AudioParams.Default.WithLoop(true))
            ?.Entity;
    }
}
