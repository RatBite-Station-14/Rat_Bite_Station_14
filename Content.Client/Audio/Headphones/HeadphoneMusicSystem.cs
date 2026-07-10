// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Audio.Headphones;
using Robust.Client.GameObjects;

namespace Content.Client.Audio.Headphones;

public sealed class HeadphoneMusicSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadphoneMusicComponent, AfterAutoHandleStateEvent>(OnAfterState);
    }

    private void OnAfterState(Entity<HeadphoneMusicComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_ui.TryGetOpenUi<HeadphoneMusicBoundUserInterface>(ent.Owner, HeadphoneMusicUiKey.Key, out var bui))
            bui.Reload();
    }
}
