using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Temperature.Systems;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChangeTemperatureOvertimeStatusEffectSystem : OvertimeStatusEffectSystem
{
    [Dependency] private readonly SharedTemperatureSystem _temperatureSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeTemperatureOvertimeStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    protected override void Tick(TimeSpan elapsedTime)
    {
        var eq = EntityQueryEnumerator<ChangeTemperatureOvertimeStatusEffectComponent, StatusEffectComponent>();
        while (eq.MoveNext(out var uid, out var changeTemp, out var effectComp))
        {
            if (effectComp.AppliedTo is not { } appliedTo) continue;
            var scale = CompOrNull<StatusEffectScaleComponent>(uid)?.Scale ?? 1f;
            _temperatureSystem.ChangeHeat(appliedTo, changeTemp.EnergyPerSecond * (float) elapsedTime.TotalSeconds * scale, ignoreHeatResistance: true);
        }
    }

    private void OnGetEffectDescription(Entity<ChangeTemperatureOvertimeStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-change-temperature", ("heatPerSecond", ent.Comp.EnergyPerSecond)));
        args.Message.PushNewline();
    }
}
