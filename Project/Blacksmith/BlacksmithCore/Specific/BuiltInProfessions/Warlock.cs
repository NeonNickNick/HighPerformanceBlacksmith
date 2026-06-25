using BlacksmithCore.Infra.Attributes.Analyzer;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Warlock : MainProfession
    {
        private static bool MagicCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Magic(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Magic());
            return DSL.CreateBy(pen);
        }

        private static bool MagicAttackCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Magic(), sc.SkillDeclareData.Param);
        }
        [HasAttack(2)]
        [HasAttack(2)]
        [HasAttack(2)]
        [IsInfinite]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile MagicAttack(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param, ResourceType.Instance.Magic())
                .WriteAttack(2 * sc.SkillDeclareData.Param, AttackType.Instance.Physical(), delayRounds: 0)
                .WriteAttack(2 * sc.SkillDeclareData.Param, AttackType.Instance.Physical(), delayRounds: 1)
                .WriteAttack(2 * sc.SkillDeclareData.Param, AttackType.Instance.Physical(), delayRounds: 2);
            return DSL.CreateBy(pen);
        }

        private bool MuteCheck(ISkillCheckContext sc) => true;
        [Labels(Impression.Aggressive, Strength.Super)]
        private static IDSLSourceFile Mute(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
               .WriteEffect(
               EffectType.Instance.AfterTransport(),
               EffectTargetType.Instance.Enemy(),
               new(),
               nameof(MuteEffectAnalyzer));
            return DSL.CreateBy(pen);
        }
        [IsAnalyzer(AnalyzerType.DSL)]
        public static void MuteEffectAnalyzer(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            enemy.Focus.Get<TurnContext>().Get<ResourceAnalyzableData>().RemoveAll(r => r.Type == ResourceType.Instance.Space() || r.Type == ResourceType.Instance.Time());
        }
        private static bool SacrificeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1;
        }
        [HasDefense]
        [HasResource]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile Sacrifice(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(1)
                .LoseMHP(1)
                .WriteDefense(new()
                {
                    Name = nameof(Sacrifice),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = 7,
                    Clock = new()
                })
                .WriteResource(1.5f, ResourceType.Instance.Iron());
            return DSL.CreateBy(pen);
        }

        private static bool AlchemyCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2.5f);
        }
        [IsEquipmentSkill]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Alchemy(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(Warlock), nameof(Alchemy).ToLower());
                    source.Focus.Get<Skill>().AddPackage(new(new Alchemy()));
                });
            return DSL.CreateBy(pen);
        }
    }
}