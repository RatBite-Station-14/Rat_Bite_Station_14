// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Ratbite.Shared.Weapons;

/// <summary>
///     Component to indicate a valid flashlight for weapon attachment
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AttachmentFlashlightComponent : AttachmentComponent;
