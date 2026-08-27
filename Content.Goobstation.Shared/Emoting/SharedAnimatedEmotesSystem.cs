// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Emoting;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Emoting;

public abstract class SharedAnimatedEmotesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<AnimatedEmotesComponent, BeforeEmoteEvent>(OnBeforeEmote);
    }

    private void OnGetState(Entity<AnimatedEmotesComponent> ent, ref ComponentGetState args)
    {
        args.State = new AnimatedEmotesComponentState(ent.Comp.Emote);
    }

    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Emote.ID != "Flip") // todo pending emote for other anims.
            return;
        var uid = ent.Owner;

        if (!TryComp<StandingStateComponent>(uid, out var standing))
        {
            args.Cancel();
            return;
        }

        if (!standing.Standing
            || HasComp<KnockedDownComponent>(uid)
            || HasComp<StunnedComponent>(uid))
        {
            args.Cancel();
            return;
        }
    }

    public void ApplyFlipEffects(EntityUid uid)
    {
        // RatBite -- Remove any flip dodge. Empty function to prevent errors.
    }
}
