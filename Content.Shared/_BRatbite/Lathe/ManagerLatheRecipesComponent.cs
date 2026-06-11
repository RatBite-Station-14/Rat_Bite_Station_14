// SPDX-FileCopyrightText: 2023 ubis1 <140386474+ubis1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Lathe.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Lathe;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ManagerLatheRecipesComponent : Component
{
    /// <summary>
    /// All of the dynamic recipe packs that the lathe is capable to get by snipping the manager wire
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<LatheRecipePackPrototype>> ManagerDynamicPacks = new();

    /// <summary>
    /// All of the static recipe packs that the lathe is capable to get by snipping the manager wire
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<LatheRecipePackPrototype>> ManagerStaticPacks = new();

    [DataField, AutoNetworkedField]
    public bool Cut = false;
}
