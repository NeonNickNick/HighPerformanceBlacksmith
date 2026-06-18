using System.Data;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Judgement
{
    public class JudgeRuleManager
    {
        public class StageRuleContainer
        {
            public class RuleUnit : IAnalyzableData
            {
                public required string AnalyzerKey { get; init; }
                public required ClapRoundClock Clock { get; init; }
                public required bool IsPlayer { get; init; }
            }
            private readonly Action<Community, Community> _baseRule;
            private readonly List<RuleUnit> _overrideRules = new();
            public readonly List<RuleUnit> _modifiersBefore = new();
            public readonly List<RuleUnit> _modifiersAfter = new();
            public StageRuleContainer(Action<Community, Community> baseRule)
            {
                _baseRule = baseRule;
            }
            public void Copy(StageRuleContainer origin)
            {
                //基础规则不需要拷贝，拷贝三个列表即可
                _overrideRules.Clear();
                foreach (var rule in origin._overrideRules)
                {
                    _overrideRules.Add(new()
                    {
                        AnalyzerKey = rule.AnalyzerKey,
                        Clock = rule.Clock.Copy(),
                        IsPlayer = rule.IsPlayer,
                    });
                }
                _modifiersBefore.Clear();
                foreach (var rule in origin._modifiersBefore)
                {
                    _overrideRules.Add(new()
                    {
                        AnalyzerKey = rule.AnalyzerKey,
                        Clock = rule.Clock.Copy(),
                        IsPlayer = rule.IsPlayer,
                    });
                }
                _modifiersAfter.Clear();
                foreach (var rule in origin._modifiersAfter)
                {
                    _overrideRules.Add(new()
                    {
                        AnalyzerKey = rule.AnalyzerKey,
                        Clock = rule.Clock.Copy(),
                        IsPlayer = rule.IsPlayer,
                    });
                }
            }
            public void AddOverride(RuleUnit ruleUnit)
            {
                _overrideRules.Add(ruleUnit);
            }
            public void AddModifier(RuleUnit ruleUnit, ModifierOrder modifierOrder)
            {
                if (modifierOrder == ModifierOrder.Before)
                {
                    _modifiersBefore.Add(ruleUnit);
                }
                else
                {
                    _modifiersAfter.Add(ruleUnit);
                }
            }
            public void Update()
            {
                _overrideRules.RemoveAll(o => o.Clock.IsDead);
                _modifiersBefore.RemoveAll(o => o.Clock.IsDead);
                _modifiersAfter.RemoveAll(o => o.Clock.IsDead);

                _overrideRules.ForEach(o => o.Clock.RoundPass());
                _modifiersBefore.ForEach(o => o.Clock.RoundPass());
                _modifiersAfter.ForEach(o => o.Clock.RoundPass());
            }
            public void Execute(Community player, Community enemy)
            {
                RuleUnit? overrideRule = null;
                for (int i = _overrideRules.Count - 1; i >= 0; i--)
                {
                    if (_overrideRules[i].Clock.IsRinging)
                    {
                        overrideRule = _overrideRules[i];
                        break;
                    }
                }
                {
                    // BEFORE modifiers
                    foreach (var rule in _modifiersBefore)
                    {
                        if (rule.Clock.IsRinging)
                        {
                            if (rule.IsPlayer)
                            {
                                AnalyzerRegistry.JudgeCallback.Get(rule.AnalyzerKey)(player, enemy);
                            }
                            else
                            {
                                AnalyzerRegistry.JudgeCallback.Get(rule.AnalyzerKey)(enemy, player);
                            }
                        }
                    }
                    // 核心规则
                    if (overrideRule == null)
                    {
                        _baseRule(player, enemy);
                    }
                    else
                    {
                        if (overrideRule.IsPlayer)
                        {
                            AnalyzerRegistry.JudgeCallback.Get(overrideRule.AnalyzerKey)(player, enemy);
                        }
                        else
                        {
                            AnalyzerRegistry.JudgeCallback.Get(overrideRule.AnalyzerKey)(enemy, player);
                        }
                    }
                    // AFTER modifiers
                    foreach (var rule in _modifiersAfter)
                    {
                        if (rule.Clock.IsRinging)
                        {
                            if (rule.IsPlayer)
                            {
                                AnalyzerRegistry.JudgeCallback.Get(rule.AnalyzerKey)(player, enemy);
                            }
                            else
                            {
                                AnalyzerRegistry.JudgeCallback.Get(rule.AnalyzerKey)(enemy, player);
                            }
                        }
                    }
                }
                Update();
            }
        }
        private readonly SortedDictionary<JudgeStage.CEValue, StageRuleContainer> _ruleContainers = new()
        {
            {
                JudgeStage.Instance.OnBegin(),
                new((player, enemy) => { })
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterAnalyzableDataWritten(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterAnalyzableDataWritten(), player, enemy))
            },
            {
                JudgeStage.Instance.OnAttackCanceling(),
                new(CancelAttacks)
            },
            {
                JudgeStage.Instance.OnApplyingEffect(),
                new(ApplyEffect)
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterTransport(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterTransport(), player, enemy))
            },
            {
                JudgeStage.Instance.OnApplyingOthers(),
                new(ApplyOthers)
            },
            {
                JudgeStage.Instance.OnUpdating(),
                new(Update)
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterResult(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResult(), player, enemy))
            },
            {
                JudgeStage.Instance.OnEnd(),
                new((player, enemy) => { })
            }
        };
        public void Copy(JudgeRuleManager origin)
        {
            foreach (var key in _ruleContainers.Keys)
            {
                _ruleContainers[key].Copy(origin._ruleContainers[key]);
            }
        }
        #region Default Rules（原有逻辑）
        private static void TakeEffects(EffectType.CEValue type, Community player, Community enemy)
        {
            foreach (var entity in player.Focus.Get<Effect>().Where(type))
            {
                if (entity.Clock.IsRinging && !entity.IsMark)
                {
                    AnalyzerRegistry.DSL.Get(entity.AnalyzerKey)(player, enemy, entity);
                }
            }

            foreach (var entity in enemy.Focus.Get<Effect>().Where(type))
            {
                if (entity.Clock.IsRinging)
                {
                    AnalyzerRegistry.DSL.Get(entity.AnalyzerKey)(enemy, player, entity);
                }
            }
        }

        private static void CancelAttacks(Community player, Community enemy)
        {
            CancelAttackAnalyzableDatas(player.Focus.Get<TurnContext>().Get<AttackAnalyzableData>(),
                                    enemy.Focus.Get<TurnContext>().Get<AttackAnalyzableData>());
        }

        private static void CancelAttackAnalyzableDatas(List<AttackAnalyzableData> playerAnalyzableDatas,
            List<AttackAnalyzableData> enemyAnalyzableDatas)
        {
            playerAnalyzableDatas = playerAnalyzableDatas.OrderBy(a => a.Type).ToList();
            enemyAnalyzableDatas = enemyAnalyzableDatas.OrderBy(a => a.Type).ToList();
            int playerIndex = 0;
            int enemyIndex = 0;

            while (playerIndex < playerAnalyzableDatas.Count && enemyIndex < enemyAnalyzableDatas.Count)
            {
                var playerAttack = playerAnalyzableDatas[playerIndex];
                var enemyAttack = enemyAnalyzableDatas[enemyIndex];

                if (playerAttack.Type == AttackType.Instance.Real() || !playerAttack.Clock.IsRinging)
                {
                    playerIndex++;
                    continue;
                }

                if (enemyAttack.Type == AttackType.Instance.Real() || !enemyAttack.Clock.IsRinging)
                {
                    enemyIndex++;
                    continue;
                }

                (playerAttack.Power, enemyAttack.Power) =
                    Cancel(playerAttack.Power, enemyAttack.Power);

                if (playerAttack.Power <= 0f)
                    playerAnalyzableDatas.RemoveAt(playerIndex);

                if (enemyAttack.Power <= 0f)
                    enemyAnalyzableDatas.RemoveAt(enemyIndex);
            }
        }
        public static (int, int) Cancel(int a, int b)
        {
            return (Math.Max(0, a - b), Math.Max(0, b - a));
        }
        private static void ApplyEffect(Community player, Community enemy)
        {
            Execute<EffectAnalyzableData>(player, enemy);
            Execute<EffectAnalyzableData>(enemy, player);
        }
        private static void ApplyOthers(Community player, Community enemy)
        {
            Execute<DefenseAnalyzableData>(player, enemy);
            Execute<DefenseAnalyzableData>(enemy, player);

            Execute<AttackAnalyzableData>(player, enemy);
            Execute<AttackAnalyzableData>(enemy, player);

            Execute<ResourceAnalyzableData>(player, enemy);
            Execute<ResourceAnalyzableData>(enemy, player);
        }
        private static void Execute<TAnalyzableData>(Community player, Community enemy)
            where TAnalyzableData : IAnalyzableData
        {
            var list = player.Focus.Get<TurnContext>().Get<TAnalyzableData>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Clock.IsRinging)
                {
                    AnalyzerRegistry.DSL.Get(list[i].AnalyzerKey)(player, enemy, list[i]);
                    list.RemoveAt(i);
                    i--;
                }
                else
                {
                    list[i].Clock.RoundPass();
                }
            }
        }
        private static void Update(Community player, Community enemy)
        {
            player.Update();
            enemy.Update();
        }
        #endregion
        public void Judge(Community player, Community enemy)
        {
            foreach (var stage in _ruleContainers)
            {
                stage.Value.Execute(player, enemy);
            }
        }

        public void AddJudgeRule(Community source, IEnumerable<ICallbackOnJudge> callbacks)
        {
            foreach (var callback in callbacks)
            {
                StageRuleContainer.RuleUnit unit = new()
                {
                    AnalyzerKey = callback.AnalyzerKey,
                    Clock = callback.Clock,
                    IsPlayer = callback.IsPlayer
                };
                if (callback is OverrideCallback overideCallback)
                {
                    _ruleContainers[overideCallback.Stage].AddOverride(unit);
                }
                else if (callback is ModifierCallback modifierCallback)
                {
                    _ruleContainers[callback.Stage].AddModifier(unit, modifierCallback.ModifierOrder);
                }
            }
        }
    }
}
