using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions.CannonDSLExtension;
using BlacksmithCore.Specific.Defense;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    namespace CannonDSLExtension
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
    public partial class Cannon : MainProfession
    {
        private static bool StrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(4)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile Strike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(4, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(TripleStrike));
            return DSL.Create(sc.Self, pen);
        }

        private static bool DoubleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2);
        }
        [HasAttack(8)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private static IDSLSourceFile DoubleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteAttack(8, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(TripleStrike));
            return DSL.Create(sc.Self, pen);
        }

        private static bool TripleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasAttack(11)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile TripleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteAttack(11, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(TripleStrike))
                .WriteResource(0.5f, ResourceType.Instance.Iron())
                .AddMark(new()
                {
                    AnalyzerKey = nameof(TripleStrike),
                    IsMark = true,
                    Type = EffectType.Instance.Default(),
                    Clock = new(isInfinite: true)
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool APShellCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(2)]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile APShell(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(2, AttackType.Instance.Physical(), 3)
                    .CompileTimeIncrease(sc.Self, nameof(TripleStrike))
                    .WithInterupt();
            return DSL.Create(sc.Self, pen);
        }

        private static bool CannonBarrelCheck(ISkillContext sc) => true;
        [HasAttack(1)]
        [HasDefense]
        [Labels(Impression.Robust, Strength.Useless)]
        private static IDSLSourceFile CannonBarrel(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(2, new CommonReduction())
                .WriteAttack(1, AttackType.Instance.Physical())
                    .CompileTimeIncrease(sc.Self, nameof(TripleStrike));

            return DSL.Create(sc.Self, pen);
        }
    }
}
