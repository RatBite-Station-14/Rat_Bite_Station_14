using System.Numerics;
using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._BRatbite.Kitchen.UI;

[UsedImplicitly]
public sealed class ContainerCookerMenuBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private ContainerCookerMenu? _menu;

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<ContainerCookerMenu>();

        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _menu.EjectButton.OnPressed += _ => SendPredictedMessage(new ContainerCookerEjectMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not ContainerCookerUpdateUserInterfaceState cState || _menu is null)
            return;

        _menu.IsBusy = EntMan.HasComponent<ActiveCookingVesselComponent>(Owner);

        _menu.SetPanelDisabled(_menu.IsBusy);

        _menu.EjectButton.Disabled = _menu.IsBusy;

        if (!EntMan.TryGetComponent<CookingVesselComponent>(Owner, out var cookingVessel))
            return;
        RebuildIngredientsList([.. cookingVessel.Storage.ContainedEntities]);
    }

    private void RebuildIngredientsList(EntityUid[] containedIngredients)
    {
        if (_menu is null)
            return;
        _menu.IngredientsList.RemoveAllChildren();
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
                SendPredictedMessage(new ContainerCookerEjectIndexedIngredientMessage(EntMan.GetNetEntity(ingredient)));
            };
            _menu.IngredientsList.AddChild(listEntry);
        }
    }
}
