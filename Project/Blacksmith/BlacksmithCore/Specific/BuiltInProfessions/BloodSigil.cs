using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class BloodSigil : MainProfession
    {
        private static bool BloodBladeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 4;
        }
        [HasAttack(4)]
        [HasRecovery]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile BloodBlade(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(4)
                .TakeMark(nameof(BloodLust), out var layerNum)
                .WriteAttack(6, AttackType.Instance.Physical())
                    .WithModify(last => last.Power = (int)MathF.Ceiling(last.Power * MathF.Pow(1.5f, layerNum.Value)))
                    .WithBloodSuck(0.75f);
            return DSL.CreateBy(pen);
        }
        private static bool BloodLustCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 2;
        }
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Super)]
        private static IDSLSourceFile BloodLust(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(2)
                .AddMark(nameof(BloodLust));
            return DSL.CreateBy(pen);
        }
        private static bool BloodRecoveryCheck(ISkillCheckContext sc) => true;
        [HasRecovery]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile BloodRecovery(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .WriteRecovery(1);
            return DSL.CreateBy(pen);
        }
        private static bool BloodShieldCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1;
        }
        [HasDefense]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile BloodShield(ISkillCheckContext sc)
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
            return DSL.CreateBy(pen);
        }
        private static bool BloodRageCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1 && sc.Self.Focus.Get<Health>().HP <= 5;
        }
        [HasAttack(5)]
        [HasRecovery]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile BloodRage(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(1)
                .TakeMark(nameof(BloodLust), out var layerNum)
                .WriteAttack(5, AttackType.Instance.Physical())
                    .WithModify(last => last.Power = (int)MathF.Ceiling(last.Power * MathF.Pow(1.5f, layerNum.Value)))
                    .WithBloodSuck(1.5f);
            return DSL.CreateBy(pen);
        }
    }
}
