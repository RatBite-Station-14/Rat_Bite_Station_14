using Content.Shared._BRatbite.Revolutionary;
using Content.Shared.Mindshield.Components;

namespace Content.Shared.Revolutionary;

public abstract partial class SharedRevolutionarySystem : EntitySystem
{
    private void SubscribeMindshieldEvents()
    {
        SubscribeLocalEvent<MindShieldComponent, ComponentShutdown>(OnMindshieldRemove);
        SubscribeLocalEvent<FakeMindShieldComponent, MapInitEvent>(OnFakeMindshieldImplanted);
        SubscribeLocalEvent<FakeMindShieldComponent, ComponentShutdown>(OnFakeMindshieldRemove);
        SubscribeLocalEvent<MindShieldComponent, MapInitEvent>(MindShieldImplanted);
    }
    private void OnMindshieldRemove(Entity<MindShieldComponent> ent, ref ComponentShutdown args)
    {
        var ev = new MindShieldChangedEvent(false, false);
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnFakeMindshieldRemove(Entity<FakeMindShieldComponent> ent, ref ComponentShutdown args)
    {
        var ev = new MindShieldChangedEvent(false, true);
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnFakeMindshieldImplanted(Entity<FakeMindShieldComponent> ent, ref MapInitEvent args)
    {
        var ev = new MindShieldChangedEvent(true, true);
        RaiseLocalEvent(ent, ref ev);
    }

}
