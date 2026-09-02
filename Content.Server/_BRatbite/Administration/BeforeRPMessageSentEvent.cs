using Robust.Shared.Player;
using static Content.Shared.Administration.SharedBwoinkSystem;

namespace Content.Server._BRatbite.Administration;

[ByRefEvent]
public record struct BeforeBwoinkMessageSentEvent(BwoinkTextMessage Message, ICommonSession SenderSession, bool IsAdmin, bool Cancelled = false);

