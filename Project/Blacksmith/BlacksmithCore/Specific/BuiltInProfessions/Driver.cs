using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Driver : MainProfession
    {
        public override IDSLSourceFile PassiveSkillImpl(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(new()
                {
                    Name = nameof(Driver),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = 1,
                    Clock = new()
                })
                .WriteDefense(new()
                {
                    Name = "TimeShield",
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = (int)MathF.Min(5, sc.Self.Focus.Get<Resource>().Query(ResourceType.Instance.Time()) * 2),
                    Clock = new()
                });
            return DSL.CreateBy(pen);
        }
        private static bool SpaceAttackCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), sc.SkillDeclareData.Param);
        }
        [HasAttack(12)]
        [IsInfinite]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile SpaceAttack(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param, ResourceType.Instance.Space())
                .TakeMark(nameof(Space2Time), out var layerNum)
                .WriteAttack(12 * sc.SkillDeclareData.Param, AttackType.Instance.Physical())
                    .WithModify(last => last.Power += layerNum.Value);
            return DSL.CreateBy(pen);
        }

        private static bool Space2TimeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 1);
        }
        [HasResource]
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Space2Time(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Space())
                .WriteResource(1, ResourceType.Instance.Time())
                .WriteDefense(new()
                {
                    Name = nameof(Space2Time),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = 3,
                    Clock = new()
                })
                .AddMark(nameof(Space2Time));
            return DSL.CreateBy(pen);
        }

        private static bool Time2SpaceCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Time(), 1);
        }
        [HasResource]
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Time2Space(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Time())
                .WriteResource(1, ResourceType.Instance.Space())
                .WriteDefense(new()
                {
                    Name = nameof(Time2Space),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = 3,
                    Clock = new()
                })
                .AddMark(nameof(Space2Time));
            return DSL.CreateBy(pen);
        }

        private static bool SpaceBarrierCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.SkillDeclareData.Param <= 5 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.SkillDeclareData.Param);
        }
        [HasDefense]
        [IsInfinite]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile SpaceBarrier(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param, ResourceType.Instance.Iron())
                .WriteDefense(new()
                {
                    Name = nameof(SpaceBarrier),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = (int)(5.5f * sc.SkillDeclareData.Param - 0.5f * sc.SkillDeclareData.Param * sc.SkillDeclareData.Param),
                    Clock = new()
                });
            return DSL.CreateBy(pen);
        }
    }
}
