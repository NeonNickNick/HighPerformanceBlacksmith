using System.Runtime.CompilerServices;
using BlacksmithCore.Infra.Attributes.Analyzer;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;
using BlacksmithCore.Specific.BuiltInProfessions.LancerDSLExtension;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    namespace LancerDSLExtension
    {
        public static class Extension
        {
            public static DSL.AttackFile WriteLancerAttack(
                this DSL.SourceFile af,
                int power,
                AttackType.CEValue attackType,
                float APFactor = 1,
                [CallerMemberName] string skillName = "")
            {
                af.TakeMark(
                    new HashSet<string>()
                    {
                    Lancer.Keys.SkyStrike,
                    Lancer.Keys.DragonTooth,
                    Lancer.Keys.TyrantDestruction,
                    Lancer.Keys.TripleStab
                    }, out var layerNums)
                    .WriteDefense(new()
                    {
                        Name = Lancer.Keys.DragonTooth,
                        AnalyzerKey = nameof(StandardAnalyzers.DefaultArmor),
                        Type = DefenseType.Instance.CommonArmor(),
                        Power = 2,
                        Clock = new()
                    },
                    ifUndo: () => layerNums.Value[Lancer.Keys.DragonTooth] <= 0)
                    .GainHP(2, ifUndo: () => layerNums.Value[Lancer.Keys.TyrantDestruction] <= 0)
                    .LoseMHP(1, ifUndo: () => layerNums.Value[Lancer.Keys.TripleStab] <= 0)
                    .WriteAttack(1, AttackType.Instance.Real(), 
                    delayRounds: 0, ifUndo: () => layerNums.Value[Lancer.Keys.TripleStab] <= 0)
                    .WriteAttack(1, AttackType.Instance.Real(), 
                    delayRounds: 1, ifUndo: () => layerNums.Value[Lancer.Keys.TripleStab] <= 0);
                return af.WriteAttack(power, attackType, APFactor: APFactor, analyzerKey: skillName)
                            .WithModify(last => last.Power += layerNums.Value[Lancer.Keys.SkyStrike] * 2);
            }
        }
    }
    public partial class Lancer : MainProfession
    {
        public static class Keys
        {
            public const string SkyStrike = nameof(SkyStrike);
            public const string DragonTooth = nameof(DragonTooth);
            public const string TyrantDestruction = nameof(TyrantDestruction);
            public const string TripleStab = nameof(TripleStab);
            public const string CounterAttack = "Counterattack";
        }
        [IsAnalyzer(AnalyzerType.DSL)]
        public static void GetPattern(Community player, Community enemy, IAnalyzableData analyzableData)
        {
            player.Focus.AddMark(analyzableData.AnalyzerKey);
        }
        private static bool SkyStrikeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(3)]
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Strong)]
        [IsAnalyzerAlias(nameof(StandardAnalyzers.DefaultAttack))]
        private static IDSLSourceFile SkyStrike(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteLancerAttack(3, AttackType.Instance.Physical())
                    .WithCallback(AttackStage.Instance.OnHitArmorFirstTime(), nameof(GetPattern))
                    .WithInterupt();
            return DSL.CreateBy(pen);
        }
        private static bool DragonToothCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(3)]
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        [IsAnalyzerAlias(nameof(StandardAnalyzers.DefaultAttack))]
        private static IDSLSourceFile DragonTooth(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteLancerAttack(3, AttackType.Instance.Physical())
                    .WithCallback(AttackStage.Instance.OnHitArmorFirstTime(), nameof(GetPattern))
                .WriteDefense(new() 
                { 
                    Name = nameof(DragonTooth),
                    AnalyzerKey = nameof(StandardAnalyzers.DefaultReduction),
                    Type = DefenseType.Instance.CommonReduction(),
                    Power = 3,
                    Clock = new()
                });
            return DSL.CreateBy(pen);
        }
        private static bool TyrantDestructionCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(3)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        [IsAnalyzerAlias(nameof(StandardAnalyzers.DefaultAttack))]
        private static IDSLSourceFile TyrantDestruction(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteLancerAttack(3, AttackType.Instance.Physical(), APFactor: 2)
                    .WithCallback(AttackStage.Instance.OnHitArmorFirstTime(), nameof(GetPattern));
            return DSL.CreateBy(pen);
        }
        private static bool TripleStabCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(2)]
        [HasAttack(2)]
        [HasAttack(1)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        [IsAnalyzerAlias(nameof(StandardAnalyzers.DefaultAttack))]
        private static IDSLSourceFile TripleStab(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteLancerAttack(2, AttackType.Instance.Physical())
                    .WithCallback(AttackStage.Instance.OnHitArmorFirstTime(), nameof(GetPattern))
                .WriteLancerAttack(2, AttackType.Instance.Physical())
                    .WithCallback(AttackStage.Instance.OnHitArmorFirstTime(), nameof(GetPattern))
                .WriteLancerAttack(1, AttackType.Instance.Physical())
                    .WithCallback(AttackStage.Instance.OnHitArmorFirstTime(), nameof(GetPattern));
            return DSL.CreateBy(pen);
        }
        private static bool RisingDragonCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.Self.Focus.CountMark(nameof(Charge)) > 0 ? 0 : 4);
        }
        [HasAttack(10)]
        [Labels(Impression.Aggressive, Strength.Strong)]
        private static IDSLSourceFile RisingDragon(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .TakeMark(nameof(Charge), out var layerNum)
                .UseResource(() => layerNum.Value > 0 ? 0 : 4, ResourceType.Instance.Iron())
                .WriteLancerAttack(9, AttackType.Instance.Magical(), skillName: nameof(StandardAnalyzers.DefaultAttack))
                    .WithModify(last => last.Power += 4 * layerNum.Value);
            return DSL.CreateBy(pen);
        }
        private static bool ChargeCheck(ISkillCheckContext sc)
        {
            var count = sc.Self.Focus.CountMark(nameof(Charge));
            return count < 2 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), count > 0 ? 0 : 4);
        }
        /*
        蓄力是目前最复杂的技能，与它相关的有三个状态变量：
            _chargeCount：蓄力层数，取值为0/1/2
            _chargeCost：蓄力消耗铁数，取值为4/0
            _wasPassive：是否是因为受到攻击被动触发，取值为false/true
        五种状态如下：
            state1: (0, 4, false)
            state2/3: (1, 0, false/true)
            state4/5: (2, 0, false/true)
        在_wasPassive为false的条件下，蓄力状态转移路径为1->2->4->1
        如果当回合判定为true，那么移动到平行true状态，之后在蓄力检测后回到1
        这一版本的蓄力只要对方在攻击抵消后还有残余攻击就会触发，且不考虑战矛可用的跨回合伤害技能（目前没有）
        以下“复位”指的是将_chargeCount设为0，_chargeCost设为4，不动_wasPassive

        反击逻辑：
        当玩家选择战矛这个职业的时候，如果不考虑炼药锅，他就不会打出跨回合伤害技能
        基于这个观察，首先在当回合攻击抵消阶段前插入一条规则，如果检测到对方有攻击，那就反击伏龙翔天
        在这之后复位，同时将_wasPassive设为true，表示刚才触发了反击
        
        蓄力检测逻辑：
        除了这个特性之外，另一个特性是如果中断蓄力那么蓄力效果就清除
        在下回合初插入一条规则检查蓄力层数，如果发现蓄力层数没有增长，说明要么这回合出的并不是蓄力，要么是上回合触发了反击
        反击时已经复位了，而反击之后下回合可以继续蓄力，因此这种情况不应该复位
        由此得到复位条件是蓄力层数没有增加并且上一回合没有触发反击
        在这之后，基于另一个观察，即蓄力检测插入的规则总是先于反击逻辑插入的规则结算，可以放心地将_wasPassive重置为false，
        如果这回合使用蓄力，又触发了反击，那么会正确地进入循环
        如果没有触发，那么就回到了state2
        如果没有使用蓄力，那么就回到了state1

        注意：前提条件是不考虑跨回合伤害技能。如果出现了战矛可用的这种技能，那么要将反击规则插入到攻击抵消之后，并且重新再做一遍抵消
        */
        [HasBuff]
        [Labels(Impression.Robust, Strength.Super)]
        private static IDSLSourceFile Charge(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .CountMark(nameof(Charge), out var layerNum)
                .UseResource(() => layerNum.Value > 0 ? 0 : 4, ResourceType.Instance.Iron())
                .AddMark(nameof(Charge))
                .RegistCallbackOnJudge(new()
                {
                    new ModifierCallback()
                    {
                        AnalyzerKey = nameof(ChargeBeforeAttackCanceling),
                        Stage = JudgeStage.Instance.OnAttackCanceling(),
                        ModifierOrder = ModifierOrder.Before,
                        Clock = new()
                    },
                    new ModifierCallback()
                    {
                        AnalyzerKey = nameof(ChargeBeforeBegin),
                        Stage = JudgeStage.Instance.OnBegin(),
                        ModifierOrder = ModifierOrder.Before,
                        Clock = new(delayRounds: 1)
                    }
                });
            return DSL.CreateBy(pen);
        }
        [IsAnalyzer(AnalyzerType.JudgeCallback)]
        public static void ChargeBeforeAttackCanceling(Community player, Community enemy)
        {
            if (enemy.Focus.Get<TurnContext>().Get<AttackAnalyzableData>().Find(a => a.Clock.IsRinging) == null)
            {
                return;
            }
            player.Focus.AddMark(Keys.CounterAttack);
            Pen pen = sf => sf
                .TakeMark(nameof(Charge), out var layerNum)
                .WriteLancerAttack(9, AttackType.Instance.Magical(), skillName: nameof(StandardAnalyzers.DefaultAttack))
                    .WithModify(last => last.Power += 4 * layerNum.Value);
            DSL.CreateBy(pen).Compile().Execute(player);
        }
        [IsAnalyzer(AnalyzerType.JudgeCallback)]
        public static void ChargeBeforeBegin(Community player, Community enemy)
        {
            var passiveLayerNum = player.Focus.TakeMark(Keys.CounterAttack);
            if (player.CurrentSkillName != nameof(Charge).ToLower() && passiveLayerNum <= 0)
            {
                player.Focus.TakeMark(nameof(Charge));
            }
            
        }
    }

}
