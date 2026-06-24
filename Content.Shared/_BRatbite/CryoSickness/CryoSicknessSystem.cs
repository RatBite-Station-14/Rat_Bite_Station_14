using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.CryoSickness;

public abstract class SharedCryoSicknessSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CryoSicknessComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CryoSicknessComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CryoSicknessComponent, DamageModifyEvent>(OnDamageModifyEvent);
        SubscribeLocalEvent<CryoSicknessComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<CryoSicknessComponent, MindAddedMessage>(OnPlayerAttach);
        SubscribeLocalEvent<CryoSicknessComponent, PlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<CryoSicknessComponent, ShakeAwakeEvent>(OnShakeAwake);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    public override void Update(float _time)
    {
        base.Update(_time);
        var entityQuery = EntityQueryEnumerator<CryoSicknessComponent>();
        while (entityQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime > comp.ExpireTime)
                RemComp<CryoSicknessComponent>(ent);
        }
    }

    private void OnMapInit(Entity<CryoSicknessComponent> ent, ref MapInitEvent args)
    {
        var duration = TimeSpan.FromMinutes(ent.Comp.DurationInMinutes);
        ent.Comp.ExpireTime = _timing.CurTime + duration;
        ent.Comp.HadPacifism = HasComp<PacifiedComponent>(ent);
        if (!_statusEffectsSystem.HasStatusEffect(ent.Owner, ent.Comp.Effect))
            _statusEffectsSystem.TryAddStatusEffectDuration(ent.Owner, ent.Comp.Effect, duration);
        EnsureComp<PacifiedComponent>(ent);
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnShutdown(Entity<CryoSicknessComponent> ent, ref ComponentShutdown args)
    {
        if (LifeStage(ent) >= EntityLifeStage.Terminating) return;
        if (!ent.Comp.HadPacifism)
        {
            _statusEffectsSystem.TryRemoveStatusEffect(ent.Owner, ent.Comp.Effect);
            RemComp<PacifiedComponent>(ent);
        }
        _popup.PopupEntity(Loc.GetString("cryosickness-end-popup"), ent.Owner, ent.Owner);
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnDamageModifyEvent(Entity<CryoSicknessComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage *= (1 - ent.Comp.DamageResistance);
    }

    private void OnPlayerAttach<T>(Entity<CryoSicknessComponent> ent, ref T args) where T : EntityEventArgs
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnDamageChange(Entity<CryoSicknessComponent> ent, ref DamageChangedEvent args)
    {
        // Ignore healing
        if (!args.DamageIncreased) return;
        if (!ent.Comp.HadPacifism)
        {
            _statusEffectsSystem.TryRemoveStatusEffect(ent.Owner, ent.Comp.Effect);
            RemComp<PacifiedComponent>(ent);
        }
        var newTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ExpireSecondsAfterDamage);
        if (newTime < ent.Comp.ExpireTime)
            ent.Comp.ExpireTime = newTime;
    }

    public void ApplyComponent(EntityUid ent)
    {
        EnsureComp<CryoSicknessComponent>(ent);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        ApplyComponent(args.Mob);
    }

    private void OnShakeAwake(Entity<CryoSicknessComponent> ent, ref ShakeAwakeEvent args)
    {
        RemComp<CryoSicknessComponent>(ent);
    }
}
