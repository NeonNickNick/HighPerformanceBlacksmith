using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Cannon : MainProfession
    {
        private static bool StrikeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(4)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Strike(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .TakeMark(nameof(TripleStrike), out var layerNum)
                .WriteAttack(4, AttackType.Instance.Physical())
                    .WithModify(last => last.Power += layerNum.Value);
            return DSL.CreateBy(pen);
        }

        private static bool DoubleStrikeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2);
        }
        [HasAttack(8)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile DoubleStrike(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .TakeMark(nameof(TripleStrike), out var layerNum)
                .WriteAttack(8, AttackType.Instance.Physical())
                    .WithModify(last => last.Power += layerNum.Value);
            return DSL.CreateBy(pen);
        }

        private static bool TripleStrikeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasAttack(11)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile TripleStrike(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .TakeMark(nameof(TripleStrike), out var layerNum)
                .WriteAttack(11, AttackType.Instance.Physical())
                    .WithModify(last => last.Power += layerNum.Value)
                .WriteResource(0.5f, ResourceType.Instance.Iron())
                .AddMark(nameof(TripleStrike));
            return DSL.CreateBy(pen);
        }

        private static bool APShellCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(2)]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile APShell(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .TakeMark(nameof(TripleStrike), out var layerNum)
                .WriteAttack(2, AttackType.Instance.Physical(), 3)
                    .WithModify(last => last.Power += layerNum.Value)
                    .WithInterupt();
            return DSL.CreateBy(pen);
        }

        private static bool CannonBarrelCheck(ISkillCheckContext sc) => true;
        [HasAttack(1)]
        [HasDefense]
        [Labels(Impression.Robust, Strength.Useless)]
        private static IDSLSourceFile CannonBarrel(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(new()
                {
                    Name = nameof(CannonBarrel),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.CommonReduction(),
                    Power = 2,
                    Clock = new()
                })
                .TakeMark(nameof(TripleStrike), out var layerNum)
                .WriteAttack(1, AttackType.Instance.Physical())
                    .WithModify(last => last.Power += layerNum.Value);

            return DSL.CreateBy(pen);
        }
    }
}
