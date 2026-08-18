// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Audio.Jukebox;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Audio.Headphones;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HeadphoneMusicComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<JukeboxPrototype>? SelectedSongId;

    [DataField, AutoNetworkedField]
    public EntityUid? AudioStream;

    [DataField]
    public float PublicMaxDistance = 2f;

    [DataField]
    public float PrivateMaxDistance = 1f;

    public float? ActiveMaxDistance;
}

[Serializable, NetSerializable]
public sealed class HeadphoneMusicSelectedMessage(ProtoId<JukeboxPrototype> songId) : BoundUserInterfaceMessage
{
    public ProtoId<JukeboxPrototype> SongId { get; } = songId;
}

[Serializable, NetSerializable]
public sealed class HeadphoneMusicSetTimeMessage(float songTime) : BoundUserInterfaceMessage
{
    public float SongTime { get; } = songTime;
}
