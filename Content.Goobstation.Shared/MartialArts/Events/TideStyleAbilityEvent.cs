// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

/// <summary>
/// Event for when a Tide Style ability action triggers
/// </summary>
public sealed partial class TideStyleAbilityEvent : InstantActionEvent
{
}

/// <summary>
/// DoAfter event for Tide Style ability charging
/// </summary>
[Serializable, NetSerializable]
public sealed partial class TideStyleAbilityDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
/// Network event sent from client to server on body part select
/// </summary>
[Serializable, NetSerializable]
public sealed class TideStyleBodyPartSelectedEvent : EntityEventArgs
{
    public NetEntity User;
    public TargetBodyPart BodyPart;

    public TideStyleBodyPartSelectedEvent(NetEntity user, TargetBodyPart bodyPart)
    {
        User = user;
        BodyPart = bodyPart;
    }
}

/// <summary>
/// Network event sent from client to server on ability cancel
/// </summary>
[Serializable, NetSerializable]
public sealed class TideStyleAbilityCancelEvent : EntityEventArgs
{
    public NetEntity User;

    public TideStyleAbilityCancelEvent(NetEntity user)
    {
        User = user;
    }
}
