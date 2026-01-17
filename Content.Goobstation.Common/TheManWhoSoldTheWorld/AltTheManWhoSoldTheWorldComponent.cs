// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Bandit <queenjess521@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Common.AltTheManWhoSoldTheWorld;

/// <summary>
/// This is used to identify an improvised Holo-Cigar user
/// </summary>
[RegisterComponent]
public sealed partial class AltTheManWhoSoldTheWorldComponent : Component
{
    [ViewVariables]
    public EntityUid? HoloCigarEntity = null;
    
    [DataField]
    public SoundSpecifier DeathAudio = new SoundPathSpecifier("/Audio/_Goobstation/Items/TheManWhoSoldTheWorld/ouchies.ogg");
}
