using Content.Shared.Chat;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChatOverTimeStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChatOverTimeStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    public override void Update(float _)
    {
        var eq = EntityQueryEnumerator<ChatOverTimeStatusEffectComponent>();
        while (eq.MoveNext(out var uid, out var chatOvertime))
        {
            if (_timing.CurTime <= chatOvertime.LastUpdate + chatOvertime.Interval) continue;
            chatOvertime.LastUpdate = _timing.CurTime;
            if (!TryComp<StatusEffectComponent>(uid, out var statusEffect) || statusEffect.AppliedTo is not { } appliedTo) continue;
            _chatSystem.TrySendInGameICMessage(appliedTo, Loc.GetString(chatOvertime.Message), chatOvertime.ChatType, false);
        }
    }

    private void OnGetDescription(Entity<ChatOverTimeStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(
            Loc.GetString($"guidebook-description-chat-{ent.Comp.ChatType switch
            {
                InGameICChatType.Speak => "speak",
                InGameICChatType.Whisper => "whisper",
                InGameICChatType.Emote => "emote",
                _ => "",
            }}")
        );
        args.Message.PushNewline();
    }
}
