namespace Content.Shared._BRatbite.Speech;

// Cancel to skip accents
[ByRefEvent]
public partial record struct OnBeforeAccentEvent(bool Cancelled = false);
