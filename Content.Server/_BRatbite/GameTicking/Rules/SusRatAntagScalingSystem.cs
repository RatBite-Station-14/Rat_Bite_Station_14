using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Server.Player;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.GameTicking.Rules;

/// <summary>
/// Randomizes SusRat roundstart antag counts from the connected population.
/// </summary>
public sealed class SusRatAntagScalingSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

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

        var query = EntityQueryEnumerator<AntagSelectionComponent, ActiveGameRuleComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var antag, out _, out var meta))
        {
            var prototypeId = meta.EntityPrototype?.ID;
            if (prototypeId == null || !SusRatRuleIds.Contains(prototypeId))
                continue;

            var heretic = prototypeId == "SusRatHeretic";
            var populationFactor = heretic ? 0.5f : 0.9f;
            var absoluteMax = heretic ? 3 : 8;
            var populationMax = (int) MathF.Round(connectedCount * populationFactor / 10f,
                MidpointRounding.AwayFromZero);
            var targetCount = _random.Next(Math.Clamp(populationMax, 0, absoluteMax) + 1);

            for (var i = 0; i < antag.Definitions.Count; i++)
            {
                var def = antag.Definitions[i];
                def.Min = targetCount;
                def.Max = targetCount;
                antag.Definitions[i] = def;
            }
        }
    }
}
