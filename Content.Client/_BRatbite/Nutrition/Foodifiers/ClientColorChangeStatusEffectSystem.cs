using Content.Shared._BRatbite.Nutrition.Foodifiers;
using Robust.Client.GameObjects;

namespace Content.Client._BRatbite.Nutrition.Foodifiers;

public sealed partial class ClientColorChangeStatusEffectSystem : ColorChangeStatusEffectSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ColorChangeComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<ColorChangeComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.AppearanceData.GetValueOrDefault(ColorStatusEffectVisuals.Color) is not Color color)
        {
            _sprite.SetColor(ent.Owner, Color.White);
        }
        else
        {
            _sprite.SetColor(ent.Owner, color);
        }
    }
}
