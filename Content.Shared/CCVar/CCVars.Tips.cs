// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Whether tips being shown is enabled at all.
    /// </summary>
    public static readonly CVarDef<bool> TipsEnabled =
        CVarDef.Create("tips.enabled", true);

    /// <summary>
    ///     The dataset prototype to use when selecting a random tip.
    /// </summary>
    public static readonly CVarDef<string> TipsDataset =
        CVarDef.Create("tips.dataset", "Tips");

    /// <summary>
    ///     The number of seconds between each tip being displayed when the round is not actively going
    ///     (i.e. postround or lobby)
    /// </summary>
    public static readonly CVarDef<float> TipFrequencyOutOfRound =
        CVarDef.Create("tips.out_of_game_frequency", 60f * 1.5f);

    /// <summary>
    ///     The number of seconds between each tip being displayed when the round is actively going
    /// </summary>
    public static readonly CVarDef<float> TipFrequencyInRound =
        CVarDef.Create("tips.in_game_frequency", 60f * 60);

    public static readonly CVarDef<string> LoginTipsDataset =
        CVarDef.Create("tips.login_dataset", "Tips");

    /// <summary>
    ///     The chance for Tippy to replace a normal tip message.
    /// </summary>
    public static readonly CVarDef<float> TipsTippyChance =
        CVarDef.Create("tips.tippy_chance", 0.1f); /// Ratbites, Tippy is based, you cant convince me otherwise.
}
