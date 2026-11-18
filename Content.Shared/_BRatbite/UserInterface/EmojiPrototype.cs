using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.UserInterface;

[Prototype]
public sealed partial class EmojiPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public SpriteSpecifier Sprite = SpriteSpecifier.Invalid;
}
