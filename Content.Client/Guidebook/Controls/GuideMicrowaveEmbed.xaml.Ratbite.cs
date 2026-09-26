using Content.Shared._BRatbite.Nutrition;
using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Kitchen;

namespace Content.Client.Guidebook.Controls;

public sealed partial class GuideMicrowaveEmbed
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    private void GenerateEffectDescription(FoodRecipePrototype recipe)
    {
        if (!_prototype.Index(recipe.Result).Components.TryGetComponent("CookedFood", out var comp) || comp is not CookedFoodComponent cookedFood) return;
        var ev = new EffectDescriptionEvent(new ());
        foreach (var statusEffect in cookedFood.StatusEffectProto)
        {
            var effect = _entityManager.Spawn(statusEffect, doMapInit: false);
            _entityManager.EventBus.RaiseLocalEvent(effect, ref ev);
            _entityManager.DeleteEntity(effect);
        }
        ResultEffectsHeader.Visible = ev.Message.Count != 0;
        ResultEffects.Text = ev.Message.ToMarkup();
    }
}
