using System.Data;
using BlacksmithCore.Infra.Attributes.Analyzer;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Common : MainProfession
    {
        private static IReadOnlySet<string> ProfessionSkillNames => ProfessionRegistry.MainProfessionSkillNames;

        private static bool IronCheck(ISkillCheckContext sc) => true;
        [HasResource]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Iron(ISkillCheckContext sc)
        {

            Pen pen = sf => sf.WriteResource(1, ResourceType.Instance.Iron());
            return DSL.CreateBy(pen);
        }

        private static bool StickCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        [HasAttack(1)]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Stick(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteAttack(1, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }

        private static bool DrillCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
        }
        [HasAttack(3)]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Drill(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteAttack(3, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }

        private static bool SlashCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2.5f);
        }
        [HasAttack(1)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Slash(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteAttack(5, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }

        private static bool ShieldCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.SkillDeclareData.Param * 0.5f);
        }
        [HasDefense]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Shield(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(new()
                {
                    Name = nameof(Shield),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.CommonReduction(),
                    Power = 2 + sc.SkillDeclareData.Param,
                    Clock = new()
                });
            return DSL.CreateBy(pen);
        }

        private static bool ThornShieldCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1 + sc.SkillDeclareData.Param * 0.5f);
        }
        [HasDefense]
        [Labels(Impression.Robust, Strength.Useless)]
        private static IDSLSourceFile ThornShield(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.SkillDeclareData.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(new()
                {
                    Name = nameof(ThornShield),
                    AnalyzerKey = nameof(StandardAnalyzers.ThornReduction),
                    Type = DefenseType.Instance.ThornReduction(),
                    Power = 4 + sc.SkillDeclareData.Param,
                    Clock = new()
                });
            return DSL.CreateBy(pen);
        }

        private static bool RecoveryCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1 + sc.SkillDeclareData.Param);
        }
        [HasRecovery]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Recovery(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.SkillDeclareData.Param, ResourceType.Instance.Iron())
                .WriteRecovery(2 + 2 * sc.SkillDeclareData.Param);
            return DSL.CreateBy(pen);
        }

        private static bool SpaceCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Space(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Space());
            return DSL.CreateBy(pen);
        }

        private static bool TimeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Useless)]
        private static IDSLSourceFile Time(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Time());
            return DSL.CreateBy(pen);
        }

        private static bool TearCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 1f);
        }
        [HasAttack(8)]
        [Labels(Impression.Aggressive, Strength.Strong)]
        private static IDSLSourceFile Tear(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Space())
                .WriteAttack(8, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }
        private static bool ReflectCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 2f);
        }
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Super)]
        private static IDSLSourceFile Reflect(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Space())
                .RegistCallbackOnJudge(
                    new()
                    {
                        new ModifierCallback()
                        {
                            AnalyzerKey = nameof(ReflectBeforeApplyingEffect),
                            Stage = JudgeStage.Instance.OnApplyingEffect(),
                            Clock = new(),
                            ModifierOrder = ModifierOrder.Before
                        },
                        new ModifierCallback()
                        {
                            AnalyzerKey = nameof(ReflectAfterAttackCanceling),
                            Stage = JudgeStage.Instance.OnAttackCanceling(),
                            Clock = new(),
                            ModifierOrder = ModifierOrder.After
                        }
                    });
            return DSL.CreateBy(pen);
        }
        [IsAnalyzer(AnalyzerType.JudgeCallback)]
        public static void ReflectBeforeApplyingEffect(Community player, Community enemy)
        {
            var playerAnalyzableDatas = player.Focus.Get<TurnContext>().Get<EffectAnalyzableData>();
            var enemyAnalyzableDatas = enemy.Focus.Get<TurnContext>().Get<EffectAnalyzableData>();

            var reflect = enemyAnalyzableDatas.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.Clock.IsRinging).ToList();

            enemyAnalyzableDatas.RemoveAll(reflect.Contains);

            foreach (var e in reflect)
            {
                e.Clock.SetDelay(1);
            }
            playerAnalyzableDatas.AddRange(reflect);
        }
        [IsAnalyzer(AnalyzerType.JudgeCallback)]
        public static void ReflectAfterAttackCanceling(Community player, Community enemy)
        {
            var tc = player.Focus.Get<TurnContext>();
            var playerAnalyzableDatas = tc.Get<AttackAnalyzableData>();
            var enemyAnalyzableDatas = enemy.Focus.Get<TurnContext>().Get<AttackAnalyzableData>();

            var reflect = enemyAnalyzableDatas.Where(a => a.Clock.IsRinging).ToList();

            enemyAnalyzableDatas.RemoveAll(reflect.Contains);

            foreach (var a in reflect)
            {
                a.Clock.SetDelay(1);
            }

            foreach (var res in reflect)
            {
                tc.WriteAnalyzableData(res);
            }
        }
        private static bool WarlockCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Warlock(ISkillCheckContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Warlock()));
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.CreateBy(pen);
        }

        private static bool CannonCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 4);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Cannon(ISkillCheckContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Cannon()));
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Instance.Iron())
                .WriteDefense(new()
                {
                    Name = nameof(Cannon),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.CommonReduction(),
                    Power = 3,
                    Clock = new()
                })
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.CreateBy(pen);
        }

        private static bool DriverCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Driver(ISkillCheckContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Driver()));
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.CreateBy(pen);
        }

        private static bool BloodSigilCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 7);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile BloodSigil(ISkillCheckContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new BloodSigil()));
            Pen pen = sf => sf
                .UseResource(7, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                    List<string> addition = new()
                    {
                        nameof(Stick).ToLower(),
                        nameof(Drill).ToLower(),
                        nameof(Slash).ToLower(),
                        nameof(Tear).ToLower()
                    };
                    addition.ForEach(a => source.Focus.Get<Skill>().RemoveSkill(nameof(Common), a));
                    source.Focus.Get<Health>().GainMHP(3);
                    source.Focus.Get<Health>().GainHP(3);
                });
            return DSL.CreateBy(pen);
        }
        private static bool LancerCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Lancer(ISkillCheckContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Lancer()));
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.CreateBy(pen);
        }
        public static void ExcludeAllProfessions(Community source)
        {
            foreach (var name in ProfessionSkillNames)
            {
                source.Focus.Get<Skill>().RemoveSkill(nameof(Common), name);
            }
        }
    }
}