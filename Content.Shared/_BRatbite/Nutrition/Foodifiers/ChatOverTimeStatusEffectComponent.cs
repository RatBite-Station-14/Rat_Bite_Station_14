using Content.Shared.Chat;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ChatOverTimeStatusEffectComponent : Component
{
    [DataField(required: true)]
    public LocId Message;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(5);

    [DataField]
    public InGameICChatType ChatType = InGameICChatType.Speak;

    [ViewVariables]
    public TimeSpan LastUpdate;
}
