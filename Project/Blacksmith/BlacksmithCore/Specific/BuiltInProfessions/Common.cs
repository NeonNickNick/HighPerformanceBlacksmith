using System.Data;
using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Common : MainProfession
    {
        private static IReadOnlySet<string> ProfessionSkillNames => ProfessionRegistry.MainProfessionSkillNames;

        private static bool IronCheck(ISkillContext sc) => true;
        [HasResource]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Iron(ISkillContext sc)
        {

            Pen pen = sf => sf.WriteResource(1, ResourceType.Instance.Iron());
            return DSL.Create(sc.Self, pen);
        }

        private static bool StickCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        [HasAttack(1)]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Stick(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteAttack(1, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private static bool DrillCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
        }
        [HasAttack(3)]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Drill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteAttack(3, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private static bool SlashCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2.5f);
        }
        [HasAttack(1)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Slash(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteAttack(5, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private static bool ShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.Param * 0.5f);
        }
        [HasDefense]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Shield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(2 + sc.Param, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }

        private static bool ThornShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1 + sc.Param * 0.5f);
        }
        [HasDefense]
        [Labels(Impression.Robust, Strength.Useless)]
        private static IDSLSourceFile ThornShield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(4 + sc.Param, new ThornReduction());
            return DSL.Create(sc.Self, pen);
        }

        private static bool RecoveryCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1 + sc.Param);
        }
        [HasRecovery]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile Recovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param, ResourceType.Instance.Iron())
                .WriteRecovery(2 + 2 * sc.Param);
            return DSL.Create(sc.Self, pen);
        }

        private static bool SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Space());
            return DSL.Create(sc.Self, pen);
        }

        private static bool TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Useless)]
        private static IDSLSourceFile Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Time());
            return DSL.Create(sc.Self, pen);
        }

        private static bool TearCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 1f);
        }
        [HasAttack(8)]
        [Labels(Impression.Aggressive, Strength.Strong)]
        private static IDSLSourceFile Tear(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Space())
                .WriteAttack(8, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        private static bool ReflectCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 2f);
        }
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Super)]
        private static IDSLSourceFile Reflect(ISkillContext sc)
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
                            IsPlayer = sc.Self.IsPlayer,
                            ModifierOrder = ModifierOrder.Before
                        },
                        new ModifierCallback()
                        {
                            AnalyzerKey = nameof(ReflectAfterAttackCanceling),
                            Stage = JudgeStage.Instance.OnAttackCanceling(),
                            Clock = new(),
                            IsPlayer = sc.Self.IsPlayer,
                            ModifierOrder = ModifierOrder.After
                        }
                    });
            return DSL.Create(sc.Self, pen);
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
        private static bool WarlockCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Warlock(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Warlock()));
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteCompileTime(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool CannonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 4);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Cannon(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Cannon()));
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Instance.Iron())
                .WriteDefense(3, new CommonReduction())
                .WriteCompileTime(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool DriverCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Driver(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Driver()));
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteCompileTime(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool BloodSigilCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 7);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile BloodSigil(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new BloodSigil()));
            Pen pen = sf => sf
                .UseResource(7, ResourceType.Instance.Iron())
                .WriteCompileTime(source =>
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
            return DSL.Create(sc.Self, pen);
        }/*
        private static bool LancerCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Lancer(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Lancer()));
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteCompileTime(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }*/
        public static void ExcludeAllProfessions(Community source)
        {
            foreach (var name in ProfessionSkillNames)
            {
                source.Focus.Get<Skill>().RemoveSkill(nameof(Common), name);
            }
        }
    }
}