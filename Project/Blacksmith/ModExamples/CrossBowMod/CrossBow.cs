
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;
using BlacksmithCore.Specific.Defense;

namespace ModExamples.CrossBowMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class CrossBow : MainProfession
    {
        private ClapStateVar<bool> _aimed = new(false);
        private bool CraftBoltCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile CraftBolt(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(3f, ResourceType.Instance.Bolt());
            return DSL.Create(sc.Self, pen);
        }
        private bool BoltVolleyCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Bolt(), sc.Param);
        }
        [HasAttack]
        private IDSLSourceFile BoltVolley(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Bolt())
                .WriteAttack(sc.Param, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        private bool AimCheck(ISkillContext sc) => true;
        private IDSLSourceFile Aim(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteCompileTime(source => _aimed.Set(true), true);
            return DSL.Create(sc.Self, pen);
        }
        private bool CriticalHitCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Bolt(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile CriticalHit(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Bolt())
                .WriteAttack(_aimed.Value ? 0f : 1f, AttackType.Instance.Physical())
                .WriteAttack(_aimed.Value ? 2f : 1f, AttackType.Instance.Real());
            _aimed.Reset();
            return DSL.Create(sc.Self, pen);
        }
        private bool ParryCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Param < 5 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.Param);
        }
        private IDSLSourceFile Parry(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param * 0.5f, ResourceType.Instance.Bolt())
                .WriteDefense(4.5f * sc.Param - 0.5f * sc.Param * sc.Param, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }
        private bool MarkingBoltCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Bolt(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile MarkingBolt(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Bolt())
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteAttack(1f, AttackType.Instance.Physical())
                .RegistCallbackOnJudge(
                    new()
                    {
                        new ModifierCallback((player, enemy) =>
                        {
                            foreach(var analyzableData in enemy.Focus.Get<TurnContext>().Get<AttackAnalyzableData>())
                            {
                                if(analyzableData.Clock.IsRinging)
                                {
                                    analyzableData.AddStage(AttackStage.Instance.OnHitBody(), (community, body, aanalyzableData) =>
                                    {
                                        aanalyzableData.Power *= 2;
                                    });
                                }
                            }
                        },
                        JudgeStage.Instance.OnApplyingOthers(),
                        ModifierOrder.Before,
                        new(remainingRounds: 1, delayRounds: 1)),
                    });
            return DSL.Create(sc.Self, pen);
        }
    }
}
