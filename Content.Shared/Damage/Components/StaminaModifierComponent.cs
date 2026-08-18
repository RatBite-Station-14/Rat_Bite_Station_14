// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StaminaModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 1f;
}
