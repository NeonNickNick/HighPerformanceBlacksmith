using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Infra.DSL
{
    public partial class StandardAnalyzers : MainProfession
    {
        [IsAnalyzer]
        public static void DefaultAttack(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            var attackData = (AttackAnalyzableData)analyzableData;
            var onHitArmorFirstTimeKeys = new List<string>();
            if (attackData.StageKeys.TryGetValue(AttackStage.Instance.OnHitArmorFirstTime(), out var value1))
            {
                onHitArmorFirstTimeKeys = value1;
            }
            var onHitBodyKeys = new List<string>();
            if (attackData.StageKeys.TryGetValue(AttackStage.Instance.OnHitBody(), out var value2))
            {
                onHitBodyKeys = value2;
            }
            var onEndKeys = new List<string>();
            if (attackData.StageKeys.TryGetValue(AttackStage.Instance.OnEnd(), out var value3))
            {
                onEndKeys = value3;
            }

            Body main = enemy.Focus;
            if (attackData.Power <= 0f)
            {
                return;
            }
            bool ifHitArmor = false;
            if (attackData.Type != AttackType.Instance.Real())
            {
                var defenses = main.Get<Defense>().Defenses;
                var APList = new List<DefenseType.CEValue>()
                        {
                            DefenseType.Instance.ThornReduction(),
                            DefenseType.Instance.CommonReduction(),
                            DefenseType.Instance.StoneShell(),
                            DefenseType.Instance.RealArmor(),
                            DefenseType.Instance.CommonArmor()
                        };
                var armorList = new List<DefenseType.CEValue>()
                        {
                            DefenseType.Instance.StoneShell(),
                            DefenseType.Instance.RealArmor(),
                            DefenseType.Instance.CommonArmor()
                        };

                foreach (var defense in defenses)
                {
                    if (!ifHitArmor && armorList.Contains(defense.Type))
                    {
                        ifHitArmor = true;

                        foreach (var key in onHitArmorFirstTimeKeys)
                        {
                            AnalyzerRegistry.DSL.Get(key)(player, enemy, analyzableData);
                        }
                    }
                    if (APList.Contains(defense.Type))
                    {
                        attackData.Power = (int)MathF.Ceiling(attackData.Power * attackData.APFactor);
                    }
                    AnalyzerRegistry.Defense.Get(defense.AnalyzerKey)(enemy, player, defense, attackData);
                    if (APList.Contains(defense.Type))
                    {
                        attackData.Power = (int)MathF.Ceiling(attackData.Power / attackData.APFactor);
                    }
                    if (attackData.Power <= 0f)
                    {
                        foreach (var key in onEndKeys)
                        {
                            AnalyzerRegistry.DSL.Get(key)(player, enemy, analyzableData);
                        }
                        return;
                    }
                }
            }
            if (!ifHitArmor)
            {
                foreach (var key in onHitArmorFirstTimeKeys)
                {
                    AnalyzerRegistry.DSL.Get(key)(player, enemy, analyzableData);
                }
            }
            foreach (var key in onHitBodyKeys)
            {
                AnalyzerRegistry.DSL.Get(key)(player, enemy, analyzableData);
            }
            main.Get<Health>().LoseHP((int)attackData.Power);
            attackData.TotalDamage += (int)attackData.Power;
            foreach (var key in onEndKeys)
            {
                AnalyzerRegistry.DSL.Get(key)(player, enemy, analyzableData);
            }
        }
        [IsAnalyzer]
        public static void DefaultBloodSuck(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            var attackData = (AttackAnalyzableData)analyzableData;
            var percent = attackData.ExtraParams[nameof(DefaultBloodSuck)];
            player.Focus.Get<Health>().GainHP((int)MathF.Ceiling(attackData.TotalDamage * percent));
        }
        [IsAnalyzer]
        public static void DefaultInterupt(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            var interuptList = new List<ResourceType.CEValue>()
                {
                    ResourceType.Instance.Iron(),
                    ResourceType.Instance.Gold_Iron(),
                    ResourceType.Instance.Magic()
                };
            enemy.Focus.Get<TurnContext>().Get<ResourceAnalyzableData>().RemoveAll(r => interuptList.Contains(r.Type));
        }

        [IsAnalyzer]
        public static void DefaultResource(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            var resourceData = (ResourceAnalyzableData)analyzableData;

            player.Focus.Get<Resource>().Gain(resourceData.Type, resourceData.Power);
        }
        [IsAnalyzer]
        public static void DefaultDefense(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            var defenseData = (DefenseAnalyzableData)analyzableData;

            defenseData.Defense.Power = defenseData.Power;
            player.Focus.Get<Defense>().Add(defenseData.Defense);
        }
    }
}
