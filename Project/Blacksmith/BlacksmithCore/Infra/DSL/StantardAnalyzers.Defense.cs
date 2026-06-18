using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Infra.DSL
{
    using DSL = DSLforSkillLogic;
    public partial class StandardAnalyzers : MainProfession
    {
        [IsAnalyzer(AnalyzerType.Defense)]
        public static void DefaultArmor(Community player, Community enemy, DefenseEntity defense, AttackAnalyzableData attackData)
        {
            var damage = Math.Min(attackData.Power, defense.Power);
            defense.Power = Math.Max(0, defense.Power - attackData.Power);
            attackData.Power -= damage;
            attackData.TotalDamage += damage;
        }
        [IsAnalyzer(AnalyzerType.Defense)]
        public static void DefaultReduction(Community player, Community enemy, DefenseEntity defense, AttackAnalyzableData attackData)
        {
            var damage = Math.Min(attackData.Power, defense.Power);
            attackData.Power -= damage;
            attackData.TotalDamage += damage;
        }
        [IsAnalyzer(AnalyzerType.Defense)]
        public static void ThornReduction(Community player, Community enemy, DefenseEntity defense, AttackAnalyzableData attackData)
        {
            var damage = Math.Min(attackData.Power, defense.Power);
            attackData.Power -= damage;
            attackData.TotalDamage += damage;
            DSL.Create(player, sf => sf.WriteAttack((int)MathF.Ceiling(damage / 2f), AttackType.Instance.Magical(), delayRounds: 1)).Compile().Execute(player);
        }
        [IsAnalyzer(AnalyzerType.Defense)]
        public static void MagicalImmunity(Community player, Community enemy, DefenseEntity defense, AttackAnalyzableData attackData)
        {
            if (attackData.Type == AttackType.Instance.Magical())
            {
                var damage = attackData.Power;
                attackData.Power = 0;
                attackData.TotalDamage += damage;
            }
        }
        [IsAnalyzer(AnalyzerType.Defense)]
        public static void PhysicalImmunity(Community player, Community enemy, DefenseEntity defense, AttackAnalyzableData attackData)
        {
            if (attackData.Type == AttackType.Instance.Physical())
            {
                var damage = attackData.Power;
                attackData.Power = 0;
                attackData.TotalDamage += damage;
            }
        }
        [IsAnalyzer(AnalyzerType.Defense)]
        public static void PercentageReduction(Community player, Community enemy, DefenseEntity defense, AttackAnalyzableData attackData)
        {
            var damage = (int)MathF.Ceiling(attackData.Power / 100f * defense.Power);
            attackData.Power -= damage;
            attackData.TotalDamage += damage;
        }
    }
}
