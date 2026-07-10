using Content.Goobstation.Shared.Emoting;
using Content.Shared.Movement.Systems;

namespace Content.Goobstation.Client.Emoting;

public sealed class JumpAbilityFlipEmoteSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, JumpAbilityPerformedEvent>(OnJumpAbilityPerformed);
    }

    private void OnJumpAbilityPerformed(Entity<AnimatedEmotesComponent> ent, ref JumpAbilityPerformedEvent args)
    {
        RaiseLocalEvent(ent.Owner, new AnimationFlipEmoteEvent());
    }
}
