
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;

namespace ModExamples.CrossBowMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class CrossBow : MainProfession
    {
        private ClapStateVar<bool> _aimed = new(false);
        private bool CraftBoltCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile CraftBolt(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(3f, ResourceType.Instance.Bolt());
            return DSL.CreateBy(pen);
        }
        private bool BoltVolleyCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Bolt(), sc.SkillDeclareData.Param);
        }
        [HasAttack]
        private IDSLSourceFile BoltVolley(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param, ResourceType.Instance.Bolt())
                .WriteAttack(sc.SkillDeclareData.Param, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }
        private bool AimCheck(ISkillCheckContext sc) => true;
        private IDSLSourceFile Aim(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source => _aimed.Set(true), true);
            return DSL.CreateBy(pen);
        }
        private bool CriticalHitCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Bolt(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile CriticalHit(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Bolt())
                .WriteAttack(_aimed.Value ? 0f : 1f, AttackType.Instance.Physical())
                .WriteAttack(_aimed.Value ? 2f : 1f, AttackType.Instance.Real());
            _aimed.Reset();
            return DSL.CreateBy(pen);
        }
        private bool ParryCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.SkillDeclareData.Param < 5 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.SkillDeclareData.Param);
        }
        private IDSLSourceFile Parry(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param * 0.5f, ResourceType.Instance.Bolt())
                .WriteDefense(4.5f * sc.SkillDeclareData.Param - 0.5f * sc.SkillDeclareData.Param * sc.SkillDeclareData.Param, new CommonReduction());
            return DSL.CreateBy(pen);
        }
        private bool MarkingBoltCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Bolt(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile MarkingBolt(ISkillExecuteContext sc)
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
            return DSL.CreateBy(pen);
        }
    }
}
