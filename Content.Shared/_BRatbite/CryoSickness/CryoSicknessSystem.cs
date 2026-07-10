using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.CryoSickness;

public abstract class SharedCryoSicknessSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    // Tag to mark entities who have had cryo sickness at some point.
    // Entities with this tag cannot be attacked by people who are
    // pacified by cryosickness.
    private readonly ProtoId<TagPrototype> _cryoSicknessTag = "CryoSickness";
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!; // Ratbite: add logs when drawing implants
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CryoSicknessComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CryoSicknessComponent, ComponentStartup>(OnStartup);
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

    private void EnsureCryoInitialized(Entity<CryoSicknessComponent> ent)
    {
        if (ent.Comp.ExpireTime == default)
        {
            var duration = TimeSpan.FromMinutes(ent.Comp.DurationInMinutes);
            ent.Comp.ExpireTime = _timing.CurTime + duration;
            ent.Comp.HadPacifism = HasComp<PacifiedComponent>(ent);

            if (!_statusEffectsSystem.HasStatusEffect(ent.Owner, ent.Comp.Effect))
                _statusEffectsSystem.TryAddStatusEffect(ent.Owner, ent.Comp.Effect, duration, true, new PacifiedComponent());
        }

        EnsureComp<PacifiedComponent>(ent);
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);
        _tagSystem.AddTag(ent, _cryoSicknessTag);
    }

    private void OnStartup(Entity<CryoSicknessComponent> ent, ref ComponentStartup args)
    {
        EnsureCryoInitialized(ent);
    }

    private void OnMapInit(Entity<CryoSicknessComponent> ent, ref MapInitEvent args)
    {
        EnsureCryoInitialized(ent);
    }

    private void OnShutdown(Entity<CryoSicknessComponent> ent, ref ComponentShutdown args)
    {
        if (LifeStage(ent) >= EntityLifeStage.Terminating) return;
        if (!ent.Comp.HadPacifism)
        {
            _statusEffectsSystem.TryRemoveStatusEffect(ent.Owner, ent.Comp.Effect);
            RemComp<PacifiedComponent>(ent);
        }
        _popup.PopupClient(Loc.GetString("cryosickness-end-popup"), ent.Owner, ent.Owner);
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnDamageModifyEvent(Entity<CryoSicknessComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage *= (1 - ent.Comp.DamageResistance);
    }

    private void OnPlayerAttach<T>(Entity<CryoSicknessComponent> ent, ref T args) where T : EntityEventArgs
    {
        EnsureCryoInitialized(ent);
    }

    private void OnDamageChange(Entity<CryoSicknessComponent> ent, ref DamageChangedEvent args)
    {
        // skip prediction as it makes some weird desyncs
        if (_timing.InPrediction) return;
        // Ignore if they are damaging themselves
        if (args.Origin == ent.Owner) return;
        // Ignore healing
        if (!args.DamageIncreased) return;
        if ((args.DamageDelta?.GetTotal() ?? 0) < ent.Comp.MinDamageBeforeRemove && args.Damageable.Damage.GetTotal() < ent.Comp.MinTotalDamageBeforeRemove) return;
        if (!ent.Comp.HadPacifism)
        {
            _statusEffectsSystem.TryRemoveStatusEffect(ent.Owner, ent.Comp.Effect);
            if (RemComp<PacifiedComponent>(ent))
            {
                // I'm not completely sold if it should be removed
                // when people take damage, I am opting to make it
                // like this to prevent people from intentionally take
                // damage instead of shaking themselves off
                // specifically so they can't get attacked by pacified
                // people.  Self inflicted damage doesn't count, but
                // there are other ways to take damage like spacing
                // yourself.
                _tagSystem.RemoveTag(ent, _cryoSicknessTag);
                _popup.PopupEntity(Loc.GetString("cryosickness-resistance-popup"), ent.Owner, ent.Owner);
            }
        }
        var newTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ExpireSecondsAfterDamage);
        if (newTime < ent.Comp.ExpireTime)
        {
            ent.Comp.ExpireTime = newTime;

        }

        if (args.Origin is null) return;
        _adminLog.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(args.Origin.Value):player} attacked {ToPrettyString(ent):player}, who still had cryo sickness");

    }

    public void ApplyComponent(EntityUid ent)
    {
        var comp = EnsureComp<CryoSicknessComponent>(ent);
        EnsureCryoInitialized((ent, comp));

    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        ApplyComponent(args.Mob);
    }

    private void OnShakeAwake(Entity<CryoSicknessComponent> ent, ref ShakeAwakeEvent args)
    {

        // If you manually shake awake, you don't deserve protection
        _tagSystem.RemoveTag(ent, _cryoSicknessTag);

        _adminLog.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(ent):player} shook themselves off cryo sickness.");

        RemComp<CryoSicknessComponent>(ent);
    }

}
