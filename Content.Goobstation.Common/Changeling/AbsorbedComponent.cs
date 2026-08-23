// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Changeling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AbsorbedComponent : Component
{
    // Ratbite: allow dehusking
    [DataField]
    public bool CanDehusk = true;

    // Ratbite: allow dehusking
    [DataField, AutoNetworkedField]
    public bool Dehusked = false;
}
