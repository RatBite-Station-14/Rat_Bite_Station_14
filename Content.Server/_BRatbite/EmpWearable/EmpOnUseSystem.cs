using Content.Server.Damage.Systems;
using Content.Server.Emp;
using Content.Shared._BRatbite.EmpGlove;
using Content.Shared.Interaction;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._BRatbite.EmpWearable;

/// <inheritdoc/>
public sealed class EmpOnUseSystem : SharedEmpOnUseSystem
{
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpOnUseComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<EmpOnUseComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        if (TryComp<ContainerManagerComponent>(args.Target, out var containerManager))
        {
            var containers = _container.GetAllContainers(args.Target.Value, containerManager);
            foreach (var container in containers)
            {
                foreach (var entity in container.ContainedEntities)
                {
                    _empSystem.TryEmpEffects(entity, ent.Comp.EmpDrain, ent.Comp.EmpDuration);
                }
            }
        }

        _empSystem.TryEmpEffects(args.Target.Value, ent.Comp.EmpDrain, ent.Comp.EmpDuration);

        var coords = _transform.GetMapCoordinates(args.Target.Value);
        Spawn(EmpSystem.EmpPulseEffectPrototype, coords);
        _audioSystem.PlayPvs(ent.Comp.EmpSound, ent);

        args.Handled = true;
    }
}
