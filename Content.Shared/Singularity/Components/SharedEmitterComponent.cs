// SPDX-FileCopyrightText: 2020 L.E.D <10257081+unusualcrow@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Remie Richards <remierichards@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 unusualcrow <unusualcrow@protonmail.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Threading;
using Content.Shared.DeviceLinking;
using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.Singularity.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EmitterComponent : Component
{
    public CancellationTokenSource? TimerCancel;

    // whether the power switch is in "on"
    [ViewVariables] public bool IsOn;
    // Whether the power switch is on AND the machine has enough power (so is actively firing)
    [ViewVariables] public bool IsPowered;

    /// <summary>
    /// counts the number of consecutive shots fired.
    /// </summary>
    [ViewVariables]
    public int FireShotCounter;

    /// <summary>
    /// The entity that is spawned when the emitter fires.
    /// </summary>
    [DataField("boltType", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BoltType = "EmitterBolt";

    [DataField("selectableTypes", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> SelectableTypes = new();

    /// <summary>
    /// The current amount of power being used.
    /// </summary>
    [DataField("powerUseActive")]
    public int PowerUseActive = 600;

    /// <summary>
    /// The amount of shots that are fired in a single "burst"
    /// </summary>
    [DataField("fireBurstSize")]
    public int FireBurstSize = 3;

    /// <summary>
    /// The time between each shot during a burst.
    /// </summary>
    [DataField("fireInterval")]
    public TimeSpan FireInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The current minimum delay between bursts.
    /// </summary>
    [DataField("fireBurstDelayMin")]
    public TimeSpan FireBurstDelayMin = TimeSpan.FromSeconds(4);

    /// <summary>
    /// The current maximum delay between bursts.
    /// </summary>
    [DataField("fireBurstDelayMax")]
    public TimeSpan FireBurstDelayMax = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The visual state that is set when the emitter is turned on
    /// </summary>
    [DataField("onState")]
    public string? OnState = "beam";

    /// <summary>
    /// The visual state that is set when the emitter doesn't have enough power.
    /// </summary>
    [DataField("underpoweredState")]
    public string? UnderpoweredState = "underpowered";

    /// <summary>
    /// Signal port that turns on the emitter.
    /// </summary>
    [DataField("onPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string OnPort = "On";

    /// <summary>
    /// Signal port that turns off the emitter.
    /// </summary>
    [DataField("offPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string OffPort = "Off";

    /// <summary>
    /// Signal port that toggles the emitter on or off.
    /// </summary>
    [DataField("togglePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string TogglePort = "Toggle";

    /// <summary>
    /// Map of signal ports to entity prototype IDs of the entity that will be fired.
    /// </summary>
    [DataField("setTypePorts", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<string, SinkPortPrototype>))]
    public Dictionary<string, string> SetTypePorts = new();

    [DataField]
    public TimeSpan PowerOffDelay = TimeSpan.FromSeconds(0);

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextWarning = TimeSpan.Zero;

    [DataField]
    public TimeSpan WarningCooldown = TimeSpan.FromSeconds(15);

    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> WarningChannels = new();
}

[NetSerializable, Serializable]
public enum EmitterVisuals : byte
{
    VisualState
}

[Serializable, NetSerializable]
public enum EmitterVisualLayers : byte
{
    Lights
}

[NetSerializable, Serializable]
public enum EmitterVisualState
{
    On,
    Underpowered,
    Off
}
