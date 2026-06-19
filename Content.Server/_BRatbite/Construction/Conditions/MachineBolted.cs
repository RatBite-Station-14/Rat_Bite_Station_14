using Content.Shared.Construction;
using JetBrains.Annotations;
using Content.Shared._BRatbite.Machines;
using Content.Shared.Examine;

namespace Content.Server._BRatbite.Construction.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    // Copy pasted from AirlockBolted.cs
    public sealed partial class MachineBolted : IGraphCondition
    {
        [DataField("value")]
        public bool Value { get; private set; } = true;

        public bool Condition(EntityUid uid, IEntityManager entityManager)
        {
            if (!entityManager.TryGetComponent(uid, out BoltableMachineComponent? machine))
                return true;

            return machine.Bolted == Value;
        }

        public bool DoExamine(ExaminedEvent args)
        {
            var entity = args.Examined;

            var entMan = IoCManager.Resolve<IEntityManager>();

            if (!entMan.TryGetComponent(entity, out BoltableMachineComponent? machine)) return false;

            if (machine.Bolted != Value)
            {
                if (Value)
                    args.PushMarkup(Loc.GetString("construction-examine-condition-airlock-bolt", ("entityName", entMan.GetComponent<MetaDataComponent>(entity).EntityName)) + "\n");
                else
                    args.PushMarkup(Loc.GetString("construction-examine-condition-airlock-unbolt", ("entityName", entMan.GetComponent<MetaDataComponent>(entity).EntityName)) + "\n");
                return true;
            }

            return false;
        }

        public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
        {
            yield return new ConstructionGuideEntry()
            {
                Localization = Value ? "construction-step-condition-airlock-bolt" : "construction-step-condition-airlock-unbolt"
            };
        }
    }
}
