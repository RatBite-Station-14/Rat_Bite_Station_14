// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Ratbite.Client.Interaction.Components;

[RegisterComponent]
// Client-side component of the HandPlaceholder. Creates and tracks a client-side entity for hand blocking visuals
public sealed partial class HandPlaceholderVisualsComponent : Component
{
    [DataField]
    public EntityUid Dummy;
}
