// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Cuffs;

public sealed class TemporaryCuffsAppliedEvent(EntityUid target) : EntityEventArgs
{
    public EntityUid Target = target;
}

public sealed class TemporaryCuffsRemovedEvent(EntityUid target) : EntityEventArgs
{
    public EntityUid Target = target;
}

public sealed class TemporaryCuffsStruggleInterruptedEvent(EntityUid target) : EntityEventArgs
{
    public EntityUid Target = target;
}
