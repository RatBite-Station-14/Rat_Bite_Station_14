// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Access.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedIdCardConsoleSystem))]
public sealed partial class IdCardConsoleComponent : Component
{
    public static string PrivilegedIdCardSlotId = "IdCardConsole-privilegedId";
    public static string TargetIdCardSlotId = "IdCardConsole-targetId";

    [DataField]
    public ItemSlot PrivilegedIdSlot = new();

    [DataField]
    public ItemSlot TargetIdSlot = new();

    [Serializable, NetSerializable]
    public sealed class WriteToTargetIdMessage : BoundUserInterfaceMessage
    {
        public readonly string FullName;
        public readonly string JobTitle;
        public readonly List<ProtoId<AccessLevelPrototype>> AccessList;
        public readonly ProtoId<JobPrototype> JobPrototype;

        // Ratbite start
        public readonly List<ProtoId<AccessLevelPrototype>> EmergencyAddedAccessList;
        public readonly List<ProtoId<AccessLevelPrototype>> EmergencyRemovedAccessList;
        // Ratbite end

        public WriteToTargetIdMessage(string fullName, string jobTitle, List<ProtoId<AccessLevelPrototype>> accessList, ProtoId<JobPrototype> jobPrototype, List<ProtoId<AccessLevelPrototype>> emergencyAddedAccessList, List<ProtoId<AccessLevelPrototype>> emergencyRemovedAccessList)
        {
            FullName = fullName;
            JobTitle = jobTitle;
            AccessList = accessList;
            JobPrototype = jobPrototype;
            EmergencyAddedAccessList = emergencyAddedAccessList;
            EmergencyRemovedAccessList = emergencyRemovedAccessList;
        }
    }

    // Put this on shared so we just send the state once in PVS range rather than every time the UI updates.

    [DataField, AutoNetworkedField]
    public List<ProtoId<AccessLevelPrototype>> AccessLevels = new()
    {
        "Armory",
        "Atmospherics",
        "Bar",
        "Brig",
        "Detective",
        "Captain",
        "Cargo",
        "Chapel",
        "Chemistry",
        "ChiefEngineer",
        "ChiefMedicalOfficer",
        "Command",
        "Cryogenics",
        "Engineering",
        "External",
        "GenpopEnter",
        "GenpopLeave",
        "HeadOfPersonnel",
        "HeadOfSecurity",
        "Hydroponics",
        "Janitor",
        "Kitchen",
        "Lawyer",
        "Maintenance",
        "Medical",
        "Quartermaster",
        "Research",
        "ResearchDirector",
        "Salvage",
        "Security",
        "Service",
        "Theatre",
        "Robotics", //Goob
        "Journalism" //Goob
    };

    [Serializable, NetSerializable]
    public sealed class IdCardConsoleBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string PrivilegedIdName;
        public readonly bool IsPrivilegedIdPresent;
        public readonly bool IsPrivilegedIdAuthorized;
        public readonly bool IsTargetIdPresent;
        public readonly string TargetIdName;
        public readonly string? TargetIdFullName;
        public readonly string? TargetIdJobTitle;
        public readonly List<ProtoId<AccessLevelPrototype>>? TargetIdAccessList;
        public readonly List<ProtoId<AccessLevelPrototype>>? AllowedModifyAccessList;
        public readonly ProtoId<JobPrototype> TargetIdJobPrototype;
        // Ratbite start
        public readonly List<ProtoId<AccessLevelPrototype>>? EmergencyAddedAccessList;
        public readonly List<ProtoId<AccessLevelPrototype>>? EmergencyRemovedAccessList;
        // Ratbite end

        public IdCardConsoleBoundUserInterfaceState(bool isPrivilegedIdPresent,
            bool isPrivilegedIdAuthorized,
            bool isTargetIdPresent,
            string? targetIdFullName,
            string? targetIdJobTitle,
            List<ProtoId<AccessLevelPrototype>>? targetIdAccessList,
            List<ProtoId<AccessLevelPrototype>>? allowedModifyAccessList,
            ProtoId<JobPrototype> targetIdJobPrototype,
            string privilegedIdName,
            string targetIdName,
            List<ProtoId<AccessLevelPrototype>>? emergencyAddedAccessList,
            List<ProtoId<AccessLevelPrototype>>? emergencyRemovedAccessList)
        {
            IsPrivilegedIdPresent = isPrivilegedIdPresent;
            IsPrivilegedIdAuthorized = isPrivilegedIdAuthorized;
            IsTargetIdPresent = isTargetIdPresent;
            TargetIdFullName = targetIdFullName;
            TargetIdJobTitle = targetIdJobTitle;
            TargetIdAccessList = targetIdAccessList;
            AllowedModifyAccessList = allowedModifyAccessList;
            TargetIdJobPrototype = targetIdJobPrototype;
            PrivilegedIdName = privilegedIdName;
            TargetIdName = targetIdName;
            // Ratbite
            EmergencyAddedAccessList = emergencyAddedAccessList;
            EmergencyRemovedAccessList = emergencyRemovedAccessList;
            // Ratbite end
        }
    }

    [Serializable, NetSerializable]
    public enum IdCardConsoleUiKey : byte
    {
        Key,
    }
}
