// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Ratbite.Server.IoC;
using Robust.Shared.ContentPack;

namespace Content.Ratbite.Server.Entry;

public sealed class EntryPoint : GameServer
{
    public override void PreInit()
    {
        ServerRatbiteContentIoC.Register(Dependencies);
    }
}
