// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Mining.UI;
using Content.Shared.Mining.BUIStates;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Mining.BUI;

[UsedImplicitly]
public sealed class OreScannerConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private OreScannerConsoleWindow? _window;

    public OreScannerConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<OreScannerConsoleWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        
        if (state is not OreScannerBoundUserInterfaceState cState)
            return;

        _window?.UpdateState(cState.State);
    }
}
