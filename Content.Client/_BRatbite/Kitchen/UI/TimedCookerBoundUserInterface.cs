using System.Numerics;
using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Client._BRatbite.Kitchen.UI;

[UsedImplicitly]
public sealed class TimedCookerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [ViewVariables]
    private TimedCookerMenu? _menu;

    [ViewVariables]
    private uint[] _calculatedTimePresets = [];

    [ViewVariables]
    private int _selectedCookPreset; // default: instant

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<TimedCookerMenu>();

        if (!EntMan.TryGetComponent<TimedCookerComponent>(Owner, out var timedCooker) ||
            !EntMan.TryGetComponent<CookingVesselComponent>(Owner, out var cookingVessel) ||
            !_proto.TryIndex(cookingVessel.PreparationMethod, out var preparationMethodPrototype))
            return;

        var sharedAudioSystem = EntMan.System<SharedAudioSystem>();
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _calculatedTimePresets =
            _menu.SetPresets(timedCooker.CookTimePresets, preparationMethodPrototype.CookTimeMultiplier);
        _menu.StartButton.OnPressed +=
            _ => SendPredictedMessage(new TimedCookerStartCookingMessage((uint) _selectedCookPreset));
        _menu.EjectButton.OnPressed += _ => SendPredictedMessage(new TimedCookerEjectMessage());

        _menu.OnCookTimeButtonPress += (args, index) =>
        {
            sharedAudioSystem.PlayPredicted(
                timedCooker.ClickSound,
                Owner,
                PlayerManager.LocalEntity!.Value, // non-null assertion probably fine BUIs are only on client
                AudioParams.Default.WithVolume(-2));
            _selectedCookPreset = index;
            _menu.CookTimeInfoLabel.Text = Loc.GetString("microwave-bound-user-interface-cook-time-label",
                ("time",
                    args.Button is TimedCookerMenu.CookTimeButton
                        ? _calculatedTimePresets[_selectedCookPreset - 1]
                        : Loc.GetString("microwave-menu-instant-button")));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not TimedCookerUpdateUserInterfaceState cState || _menu is null)
            return;

        _menu.IsBusy = cState.RecipeEnd is not null;
        _menu.RecipeEnd = cState.RecipeEnd;

        _menu.SetPanelDisabled(_menu.IsBusy);

        _menu.StartButton.Disabled = _menu.IsBusy;
        _menu.EjectButton.Disabled = _menu.IsBusy;

        if (!EntMan.TryGetComponent<CookingVesselComponent>(Owner, out var cookingVesselComponent))
            return;
        RebuildIngredientsList([.. cookingVesselComponent.Storage.ContainedEntities]);
    }

    private void RebuildIngredientsList(EntityUid[] containedIngredients)
    {
        if (_menu is null)
            return;
        _menu.IngredientsList.DisposeAllChildren();
        foreach (var ingredient in containedIngredients)
        {
            if (EntMan.Deleted(ingredient))
                continue;

            var button = new Button
            {
                HorizontalExpand = true,
                Text = EntMan.GetComponent<MetaDataComponent>(ingredient).EntityName,
            };

            var listEntry = new BoxContainer
            {
                Align = BoxContainer.AlignMode.Begin,
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Children =
                {
                    new SpriteView(ingredient, EntMan)
                    {
                        SetSize = new Vector2(32, 32),
                    },
                    button,
                },
            };
            button.OnPressed += _ =>
            {
                SendPredictedMessage(new TimedCookerEjectIndexedIngrediantMessage(EntMan.GetNetEntity(ingredient)));
            };
            _menu.IngredientsList.AddChild(listEntry);
        }
    }
}
