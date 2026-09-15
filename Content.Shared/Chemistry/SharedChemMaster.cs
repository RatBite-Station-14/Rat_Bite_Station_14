// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public sealed class SharedChemMaster
    {
        public const uint PillTypes = 20;
        // Ratbite: This was removed, use the methods in the ratbite
        // partial file instead
        // public const string BufferSolutionName = "buffer";
        public const string InputSlotName = "beakerSlot";
        public const string OutputSlotName = "outputSlot";
        public const string PillSolutionName = "food";
        public const string BottleSolutionName = "drink";
        public const uint LabelMaxLength = 50;
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterSetModeMessage : BoundUserInterfaceMessage
    {
        public readonly ChemMasterMode ChemMasterMode;

        public ChemMasterSetModeMessage(ChemMasterMode mode)
        {
            ChemMasterMode = mode;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterSetPillTypeMessage : BoundUserInterfaceMessage
    {
        public readonly uint PillType;

        public ChemMasterSetPillTypeMessage(uint pillType)
        {
            PillType = pillType;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterReagentAmountButtonMessage : BoundUserInterfaceMessage
    {
        public readonly ReagentId ReagentId;
        public readonly ChemMasterReagentAmount Amount;
        public readonly bool FromBuffer;

        public ChemMasterReagentAmountButtonMessage(ReagentId reagentId, ChemMasterReagentAmount amount, bool fromBuffer)
        {
            ReagentId = reagentId;
            Amount = amount;
            FromBuffer = fromBuffer;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterCreatePillsMessage : BoundUserInterfaceMessage
    {
        public readonly uint Dosage;
        public readonly uint Number;
        public readonly string Label;

        public ChemMasterCreatePillsMessage(uint dosage, uint number, string label)
        {
            Dosage = dosage;
            Number = number;
            Label = label;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterOutputToBottleMessage : BoundUserInterfaceMessage
    {
        public readonly uint Dosage;
        public readonly string Label;

        public ChemMasterOutputToBottleMessage(uint dosage, string label)
        {
            Dosage = dosage;
            Label = label;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterOutputDrawSourceMessage(ChemMasterDrawSource drawSource) : BoundUserInterfaceMessage
    {
        public readonly ChemMasterDrawSource DrawSource = drawSource;
    }

    public enum ChemMasterMode
    {
        Transfer,
        Discard,
    }

    public enum ChemMasterSortingType : byte
    {
        None = 0,
        Alphabetical = 1,
        Quantity = 2,
        Latest = 3,
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterSortingTypeCycleMessage : BoundUserInterfaceMessage;


    public enum ChemMasterReagentAmount
    {
        U1 = 1,
        U5 = 5,
        U10 = 10,
        U15 = 15,
        U20 = 20,
        U25 = 25,
        U30 = 30,
        U50 = 50,
        U100 = 100,
        All,
    }

    public enum ChemMasterDrawSource
    {
        Internal,
        External,
    }

    public static class ChemMasterReagentAmountToFixedPoint
    {
        public static FixedPoint2 GetFixedPoint(this ChemMasterReagentAmount amount)
        {
            if (amount == ChemMasterReagentAmount.All)
                return FixedPoint2.MaxValue;
            else
                return FixedPoint2.New((int)amount);
        }
    }

    /// <summary>
    /// Information about the capacity and contents of a container for display in the UI
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ContainerInfo
    {
        /// <summary>
        /// The container name to show to the player
        /// </summary>
        public readonly string DisplayName;

        /// <summary>
        /// The currently used volume of the container
        /// </summary>
        public readonly FixedPoint2 CurrentVolume;

        /// <summary>
        /// The maximum volume of the container
        /// </summary>
        public readonly FixedPoint2 MaxVolume;

        /// <summary>
        /// A list of the entities and their sizes within the container
        /// </summary>
        public List<(string Id, FixedPoint2 Quantity)>? Entities { get; init; }

        public List<ReagentQuantity>? Reagents { get; init; }

        public ContainerInfo(string displayName, FixedPoint2 currentVolume, FixedPoint2 maxVolume)
        {
            DisplayName = displayName;
            CurrentVolume = currentVolume;
            MaxVolume = maxVolume;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChemMasterBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly ContainerInfo? InputContainerInfo;
        public readonly ContainerInfo? OutputContainerInfo;

        /// <summary>
        /// A list of the reagents and their amounts within the buffer, if applicable.
        /// </summary>
        public readonly List<ReagentQuantityTemperature> BufferReagents; // Ratbite: added temperature

        public readonly ReagentId? SelectedReagentToHeat; // Ratbite

        public float TargetTemperature; // Ratbite

        public readonly ChemMasterMode Mode;

        public readonly ChemMasterSortingType SortingType;

        public readonly FixedPoint2? BufferCurrentVolume;
        public readonly uint SelectedPillType;

        public readonly uint PillDosageLimit;

        public readonly bool UpdateLabel;

        public readonly ChemMasterDrawSource DrawSource;

        public ChemMasterBoundUserInterfaceState(
            ChemMasterMode mode, ChemMasterSortingType sortingType, ContainerInfo? inputContainerInfo, ContainerInfo? outputContainerInfo,
            List<ReagentQuantityTemperature> bufferReagents, FixedPoint2 bufferCurrentVolume,
            uint selectedPillType, uint pillDosageLimit, bool updateLabel, ChemMasterDrawSource drawSource, ReagentId? selectedReagentToHeat, float targetTemperature)
        {
            InputContainerInfo = inputContainerInfo;
            OutputContainerInfo = outputContainerInfo;
            BufferReagents = bufferReagents;
            Mode = mode;
            SortingType = sortingType;
            BufferCurrentVolume = bufferCurrentVolume;
            SelectedPillType = selectedPillType;
            PillDosageLimit = pillDosageLimit;
            UpdateLabel = updateLabel;
            DrawSource = drawSource;
            SelectedReagentToHeat = selectedReagentToHeat;
            TargetTemperature = targetTemperature;
        }
    }

    [Serializable, NetSerializable]
    public enum ChemMasterUiKey
    {
        Key
    }

    // Ratbite start: Reagent quantity with temperature
    [Serializable, NetSerializable]
    public record struct ReagentQuantityTemperature(ReagentId Reagent, FixedPoint2 Quantity, float Temperature)
    {
        public ReagentQuantity ToReagentQuantity()
        {
            return new ReagentQuantity(Reagent, Quantity);
        }
    }

    [Serializable, NetSerializable]
    public class ChemMasterSelectReagentToHeatMessage : BoundUserInterfaceMessage
    {
        public ReagentId? Reagent;

        public ChemMasterSelectReagentToHeatMessage(ReagentId? reagent)
        {
            Reagent = reagent;
        }
    }

    [Serializable, NetSerializable]
    public class ChemMasterSetHeatMessage : BoundUserInterfaceMessage
    {
        public float Temperature;

        public ChemMasterSetHeatMessage(float temperature)
        {
            Temperature = temperature;
        }
    }
    // Ratbite end
}
