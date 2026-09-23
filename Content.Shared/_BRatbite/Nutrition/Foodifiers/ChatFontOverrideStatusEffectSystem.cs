using Content.Shared.Chat;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChatFontOverrideStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChatFontOverrideStatusEffectComponent, StatusEffectRelayedEvent<TransformSpeakerFontEvent>>(OnChatFontOverride);
    }

    private void OnChatFontOverride(Entity<ChatFontOverrideStatusEffectComponent> ent, ref StatusEffectRelayedEvent<TransformSpeakerFontEvent> args)
    {
        if (ent.Comp.Color is { } color)
            args.Args.Color = color;
        if (ent.Comp.FontId is { } fontId)
            args.Args.FontId = fontId;
    }
}
