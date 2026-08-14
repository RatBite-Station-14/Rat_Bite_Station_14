namespace Content.Shared._BRatbite.Revolutionary;

[ByRefEvent]
public record struct MindShieldChangedEvent(bool Active, bool Fake);
