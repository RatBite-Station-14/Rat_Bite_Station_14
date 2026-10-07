using Content.Client._BRatbite.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Chemistry.UI;

public sealed partial class ChemMasterWindow
{
    private ReagentId? _selectedReagentToHeat;
    private float _targetHeat;
    public event Action<ReagentId?>? OnReagentSelected;
    public event Action<float>? OnThermostatChanged;
    private Dictionary<ReagentId, Label> _temperatureLabels = new ();
    private static float DefaultTemperature = 293f;
    private static float MaxTemp = 1000f;
    private EntityUid? _chemMaster;

    private void SetupThermostat()
    {
        Thermostat.Text = DefaultTemperature.ToString();
        Thermostat.OnTextEntered += (args) =>
        {
            float temp = DefaultTemperature;
            float.TryParse(args.Text, out temp);
            UpdateThermostat(temp);
            OnThermostatChanged?.Invoke(_targetHeat);
        };

        DecreaseTempRange.OnPressed += (_) =>
        {
            UpdateThermostat(_targetHeat - 5f);
            OnThermostatChanged?.Invoke(_targetHeat);
        };

        IncreaseTempRange.OnPressed += (_) =>
        {
            UpdateThermostat(_targetHeat + 5f);
            OnThermostatChanged?.Invoke(_targetHeat);
        };
    }

    private void UpdateThermostat(float temperature)
    {
        temperature = float.Clamp(temperature, 0, MaxTemp);
        _targetHeat = temperature;
        Thermostat.Text = _targetHeat.ToString();
    }

    protected override void Draw(DrawingHandleScreen _)
    {
        base.Draw(_);
        if (_chemMaster is not { } chemMaster) return;
        var chemMasterComp = _entityManager.GetComponent<ChemMasterComponent>(chemMaster);
        foreach (var (reagent, reagentData) in chemMasterComp.Reagents)
        {
            if (!_temperatureLabels.TryGetValue(reagent, out var label)) continue;

            label.Text = Loc.GetString("chem-master-temperature-label", [("temperature", MathF.Round(reagentData.Temperature))]);
        }
    }
}
