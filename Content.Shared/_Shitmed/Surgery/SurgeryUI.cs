// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared._Shitmed.Medical.Surgery;

[Serializable, NetSerializable]
public enum SurgeryUIKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class SurgeryPartBuiData(
    NetEntity part,
    TargetBodyPart targetPart,
    string name,
    WoundableSeverity severity,
    List<EntProtoId> surgeries)
{
    public readonly NetEntity Part = part;
    public readonly TargetBodyPart TargetPart = targetPart;
    public readonly string Name = name;
    public readonly WoundableSeverity Severity = severity;
    public readonly List<EntProtoId> Surgeries = surgeries;
}

[Serializable, NetSerializable]
public sealed class SurgeryBuiState(
    Dictionary<NetEntity, List<EntProtoId>> choices,
    Dictionary<TargetBodyPart, SurgeryPartBuiData> parts,
    string patientName,
    string speciesName) : BoundUserInterfaceState
{
    public readonly Dictionary<NetEntity, List<EntProtoId>> Choices = choices;
    public readonly Dictionary<TargetBodyPart, SurgeryPartBuiData> Parts = parts;
    public readonly string PatientName = patientName;
    public readonly string SpeciesName = speciesName;
}

[Serializable, NetSerializable]
public sealed class SurgeryBuiRefreshMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class SurgeryStepChosenBuiMsg(NetEntity part, EntProtoId surgery, EntProtoId step, bool isBody) : BoundUserInterfaceMessage
{
    public readonly NetEntity Part = part;
    public readonly EntProtoId Surgery = surgery;
    public readonly EntProtoId Step = step;

    // Used as a marker for whether or not we're hijacking surgery by applying it on the body itself.
    public readonly bool IsBody = isBody;
}
