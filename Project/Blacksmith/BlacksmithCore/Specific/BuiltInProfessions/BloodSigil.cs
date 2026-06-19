using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions.BloodSigilDSLExtension;
namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    namespace BloodSigilDSLExtension
    {
        public static class Extension
        {
            private const float MultiFactor = 1.5f;
            private static int IncreaseAttack(int origin)
            {
                var res = (int)MathF.Ceiling(origin * MultiFactor);
                return res;
            }
            public static DSL.AttackFile CompileTimeIncrease(this DSL.AttackFile af, Community self, string markName)
            {
                return af.WithComplieTime(last =>
                {
                    var marks = self.Focus.Get<Effect>().Effects;
                    var index = marks.FindIndex(m => m.AnalyzerKey == markName);
                    if (index == -1)
                    {
                        return;
                    }
                    last.Power = IncreaseAttack(last.Power);
                    marks.RemoveAt(index);
                });
            }
        }
    }
    public partial class BloodSigil : MainProfession
    {
        private static bool BloodBladeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 4;
        }
        [HasAttack(4)]
        [HasRecovery]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile BloodBlade(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(4)
                .WriteAttack(6, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(BloodLust))
                    .WithBloodSuck(0.75f);
            return DSL.Create(sc.Self, pen);
        }
        private static bool BloodLustCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 2;
        }
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Super)]
        private static IDSLSourceFile BloodLust(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(2)
                .AddMark(new()
                {
                    AnalyzerKey = nameof(BloodLust),
                    IsMark = true,
                    Type = EffectType.Instance.Default(),
                    Clock = new(isInfinite: true)
                });
            return DSL.Create(sc.Self, pen);
        }
        private static bool BloodRecoveryCheck(ISkillContext sc) => true;
        [HasRecovery]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile BloodRecovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteRecovery(1);
            return DSL.Create(sc.Self, pen);
        }
        private static bool BloodShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1;
        }
        [HasDefense]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile BloodShield(ISkillContext sc)
        {
            int power = (int)MathF.Ceiling(0.4f * sc.Self.Focus.Get<Health>().HP);
            Pen pen = sf => sf
                .LoseHP(1)
                .WriteDefense(new()
                {
                    Name = nameof(BloodShield),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.CommonReduction(),
                    Power = power,
                    Clock = new()
                });
            return DSL.Create(sc.Self, pen);
        }
        private static bool BloodRageCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1 && sc.Self.Focus.Get<Health>().HP <= 5;
        }
        [HasAttack(5)]
        [HasRecovery]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile BloodRage(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(1)
                .WriteAttack(5, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(BloodLust))
                    .WithBloodSuck(1.5f);
            return DSL.Create(sc.Self, pen);
        }
    }
}
