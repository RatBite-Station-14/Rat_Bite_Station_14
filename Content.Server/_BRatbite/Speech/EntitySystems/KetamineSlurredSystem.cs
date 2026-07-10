using System.Text;
using Content.Server._BRatbite.Speech.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.Speech.EntitySystems;

public sealed class KetamineSlurredSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KetamineSlurredAccentComponent, AccentGetEvent>(OnAccent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<KetamineSlurredAccentComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!ShouldSuppressWithCharcoal(uid))
                continue;

            RemCompDeferred<KetamineSlurredAccentComponent>(uid);
        }
    }

    private void OnAccent(EntityUid uid, KetamineSlurredAccentComponent component, AccentGetEvent args)
    {
        if (ShouldSuppressWithCharcoal(uid))
            return;

        args.Message = Accentuate(args.Message);
    }

    private bool ShouldSuppressWithCharcoal(EntityUid uid)
    {
        return _solutions.GetTotalPrototypeQuantity(uid, "Charcoal").Float() >= 10f;
    }

    private string Accentuate(string message)
    {
        var sb = new StringBuilder();

        foreach (var character in message)
        {
            if (_random.Prob(0.65f))
            {
                var lower = char.ToLowerInvariant(character);
                var newString = lower switch
                {
                    'o' => "oo",
                    's' => "sh",
                    'a' => "ah",
                    'u' => "oo",
                    'c' => "k",
                    'r' => "rr",
                    _ => $"{character}",
                };

                sb.Append(newString);
            }

            if (_random.Prob(0.2f))
            {
                if (character == ' ')
                    sb.Append("... ");
                else if (character == '.')
                    sb.Append(" *hic*");
            }

            if (!_random.Prob(0.35f))
            {
                sb.Append(character);
                continue;
            }

            var next = _random.Next(1, 4) switch
            {
                1 => "'",
                2 => $"{character}{character}",
                _ => $"{character}{character}{character}",
            };

            sb.Append(next);
        }

        return sb.ToString();
    }
}
