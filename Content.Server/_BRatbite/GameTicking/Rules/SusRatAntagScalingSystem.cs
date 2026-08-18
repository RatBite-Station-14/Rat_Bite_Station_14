using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Server.Player;

namespace Content.Server._BRatbite.GameTicking.Rules;

/// <summary>
/// Scales SusRat roundstart antag counts using connected population with a 35% reduction.
/// </summary>
public sealed class SusRatAntagScalingSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private static readonly HashSet<string> SusRatRuleIds =
    [
        "SusRatTraitor",
        "SusRatChangeling",
        "SusRatHeretic"
    ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning, before: [typeof(AntagSelectionSystem)]);
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var connectedCount = _antag.GetTotalPlayerCount(_player.Sessions);
        var scaledCount = Math.Max(0, (int) MathF.Floor(connectedCount * 0.65f));

        var query = EntityQueryEnumerator<AntagSelectionComponent, ActiveGameRuleComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var antag, out _, out var meta))
        {
            var prototypeId = meta.EntityPrototype?.ID;
            if (prototypeId == null || !SusRatRuleIds.Contains(prototypeId))
                continue;

            for (var i = 0; i < antag.Definitions.Count; i++)
            {
                var def = antag.Definitions[i];
                var targetCount = _antag.GetTargetAntagCount((uid, antag), scaledCount, def);
                targetCount = Math.Clamp(targetCount, def.Min, def.Max);
                def.Min = targetCount;
                def.Max = targetCount;
                antag.Definitions[i] = def;
            }
        }
    }
}
