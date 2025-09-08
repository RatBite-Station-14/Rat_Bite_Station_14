using System.Diagnostics.CodeAnalysis;
using System.Text;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Content.Shared.Roles;

namespace Content.Shared._Omu.Roles;

/// <summary>
/// Requires a character to have a certain fixture mass
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class WeightRequirement : JobRequirement
{
    [DataField(required: true)]
    public float MinimumWeight = 0;

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        //the profile could be null if the player is a ghost. In this case we don't need to block the role selection for ghostrole
        if (profile is null)
            return true;

        // get the fixure component belonging to the player's species
        var species = protoManager.Index(profile.Species);
        protoManager.Index(species.Prototype).TryGetComponent<FixturesComponent>(out var fixture, entManager.ComponentFactory);

        if (fixture == null)
        {
            return false;
        }

        // "fix1" is used for all collisions except for getting set on fire iirc.
        var fixtureMass = FixtureSystem.GetMassData(fixture.Fixtures["fix1"].Shape, fixture.Fixtures["fix1"].Density).Mass;
        fixtureMass *= (profile.Width + profile.Height) / 2; // Alter the result of the fixture mass calculation based on the character's scale.

        if (!Inverted)
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-below-weight", ("weight", MinimumWeight)));
            return fixtureMass >= MinimumWeight;
        }
        else
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-above-weight", ("weight", MinimumWeight)));
            return fixtureMass <= MinimumWeight;
        }
    }

}