using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;
using BlacksmithCore.Specific.BuiltInProfessions.DriverDSLExtension;
namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    namespace DriverDSLExtension
    {
        public static class Extension
        {
            public static DSL.AttackFile CompileTimeIncrease(this DSL.AttackFile af, Community self, string markName)
            {
                return af.WithComplieTime(last =>
                {
                    var marks = self.Focus.Get<Effect>().Effects;
                    var t = marks.FindAll(m => m.AnalyzerKey == markName);
                    if (t == null)
                    {
                        return;
                    }
                    last.Power += t.Count;
                    marks.RemoveAll(t.Contains);
                });
            }
        }
    }
    public partial class Driver : MainProfession
    {
        private readonly ClapStateVar<int> _pending = new(0);
        private int AttackPower(int basePower)
        {
            if (_pending.Value <= 0)
            {
                return basePower;
            }

            var result = basePower + _pending.Value;
            _pending.Reset();
            return result;
        }
        public override IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
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
            return DSL.Create(sc.Self, pen);
        }
        private static bool SpaceAttackCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), sc.Param);
        }
        [HasAttack(12)]
        [IsInfinite]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile SpaceAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Space())
                .WriteAttack(12 * sc.Param, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(Space2Time));
            return DSL.Create(sc.Self, pen);
        }

        private static bool Space2TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 1);
        }
        [HasResource]
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Space2Time(ISkillContext sc)
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
                .AddMark(new()
                {
                    AnalyzerKey = nameof(Space2Time),
                    IsMark = true,
                    Type = EffectType.Instance.Default(),
                    Clock = new(isInfinite: true)
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool Time2SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Time(), 1);
        }
        [HasResource]
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile Time2Space(ISkillContext sc)
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
                .AddMark(new()
                {
                    AnalyzerKey = nameof(Space2Time),
                    IsMark = true,
                    Type = EffectType.Instance.Default(),
                    Clock = new(isInfinite: true)
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool SpaceBarrierCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Param <= 5 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.Param);
        }
        [HasDefense]
        [IsInfinite]
        [Labels(Impression.Conservative, Strength.Useless)]
        private static IDSLSourceFile SpaceBarrier(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Iron())
                .WriteDefense(new()
                {
                    Name = nameof(SpaceBarrier),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.RealReduction(),
                    Power = (int)(5.5f * sc.Param - 0.5f * sc.Param * sc.Param),
                    Clock = new()
                });
            return DSL.Create(sc.Self, pen);
        }
    }
}
