using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Utility;

namespace Content.Ratbite.Shared.Roles.Requirements;

[DataDefinition]
public sealed partial class CreditRequirement : JobRequirement
{
    [DataField("credits", required: true)]
    public int RequiredCredits;

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();
        var currentCredits = profile?.Credits;

        if (currentCredits >= RequiredCredits)
            return true;

        reason = FormattedMessage.FromMarkupPermissive($"Requires [color=red]{RequiredCredits}[/color] Meta-Credits.");
        return false;
    }
}
