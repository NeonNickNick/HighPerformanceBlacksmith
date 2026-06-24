using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using ModExamples.CauldronMod.Defense;

namespace ModExamples.CauldronMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Cauldron : MainProfession
    {
        private bool FireCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Fire(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Fire());
            return DSL.CreateBy(pen);
        }
        private bool WaterCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Water(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Water());
            return DSL.CreateBy(pen);
        }
        private bool WoodCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Wood(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Wood());
            return DSL.CreateBy(pen);
        }
        private bool EarthCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Earth(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Earth());
            return DSL.CreateBy(pen);
        }
        private bool ExplosionCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Fire(), sc.SkillDeclareData.Param);
        }
        [HasAttack]
        private IDSLSourceFile Explosion(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param, ResourceType.Instance.Fire())
                .WriteAttack(4f * sc.SkillDeclareData.Param, AttackType.Instance.Magical());
            return DSL.CreateBy(pen);
        }
        private bool IceBladeCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Water(), sc.SkillDeclareData.Param);
        }
        [HasAttack]
        private IDSLSourceFile IceBlade(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.SkillDeclareData.Param, ResourceType.Instance.Water())
                .WriteAttack(5f * sc.SkillDeclareData.Param, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }
        private bool RegenerationCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wood(), 1f);
        }
        private IDSLSourceFile Regeneration(ISkillCheckContext sc)
        {
            float begin = 1f;
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Wood())
                .WriteEffect(EffectType.Instance.AfterAnalyzableDataWritten()
                            , EffectTargetType.Instance.Self()
                            , begin
                            , 3
                            , (Community source, Body target, EffectEntity effectEntity) =>
                            {
                                target.Get<Health>().GainHP((int)effectEntity.Power);
                                begin++;
                                effectEntity.Power = begin;
                            });
            return DSL.CreateBy(pen);
        }
        private bool StoneShellCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Earth(), 1f);
        }
        private IDSLSourceFile StoneShell(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Earth())
                .WriteDefense(0f, new StoneShell());
            return DSL.CreateBy(pen);
        }
        private bool LifeBurnCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wood(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Fire(), 1f);
        }
        private IDSLSourceFile LifeBurn(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Fire())
                .UseResource(1f, ResourceType.Instance.Wood())
                .WriteDefense(1, new PercentageReduction(baseline: 2), delayRounds: 0)
                .WriteDefense(1, new PercentageReduction(baseline: 2), delayRounds: 1)
                .WriteDefense(1, new PercentageReduction(baseline: 2), delayRounds: 2)
                .WriteDefense(1, new PercentageReduction(baseline: 2), delayRounds: 3)
                .RegistCallbackOnJudge(
                    new()
                    {
                        new ModifierCallback((player, enemy) =>
                        {
                            foreach(var analyzableData in player.Focus.Get<TurnContext>().Get<AttackAnalyzableData>())
                            {
                                if(analyzableData.Type == AttackType.Instance.Real()
                                && analyzableData.Clock.IsRinging)
                                {
                                    analyzableData.Power /= 2;
                                }
                            }
                        },
                        JudgeStage.Instance.OnApplyingOthers(),
                        ModifierOrder.Before,
                        new(remainingRounds: 4)),
                        new ModifierCallback((player, enemy) =>
                        {
                            foreach(var analyzableData in player.Focus.Get<TurnContext>().Get<AttackAnalyzableData>())
                            {
                                if(analyzableData.Clock.IsRinging)
                                {
                                    analyzableData.Power *= 2;
                                }
                            }
                        },
                        JudgeStage.Instance.OnBegin(),
                        ModifierOrder.After,
                        new(remainingRounds: 3, delayRounds: 1)),
                        new ModifierCallback((player, enemy) =>
                        {
                            player.Focus.Get<Health>().LoseMHP(114514);
                        },
                        JudgeStage.Instance.OnEnd(),
                        ModifierOrder.Before,
                        new(remainingRounds: 1, delayRounds: 3)),
                    });
            return DSL.CreateBy(pen);
        }

        private bool FireRainCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Earth(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Fire(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile FireRain(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Fire())
                .UseResource(1f, ResourceType.Instance.Earth())
                .WriteAttack(8f, AttackType.Instance.Physical(), delayRounds: 0)
                .WriteAttack(2f, AttackType.Instance.Real(), delayRounds: 0)
                .WriteAttack(1f, AttackType.Instance.Real(), delayRounds: 0);
            return DSL.CreateBy(pen);
        }
        private bool ElementalArmorCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Earth(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Water(), 1f);
        }
        private IDSLSourceFile ElementalArmor(ISkillCheckContext sc)
        {
            HashSet<string> packageNames = new();
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Earth())
                .UseResource(1f, ResourceType.Instance.Water())
                .WriteDefense(0f, new PhysicalImmunity())
                .WriteDefense(8f, new Defense.ElementalArmor(owner =>
                {
                    owner.Focus.Get<Skill>().RemovePackage(nameof(ElementalArmor));
                    owner.Focus.Get<Skill>().Enable(packageNames);
                }))
                .WriteFree(source =>
                {
                    packageNames = source.Focus.Get<Skill>().DisableAll();
                    source.Focus.Get<Skill>().AddPackage(new(new ElementalArmor()));
                }, true);
            return DSL.CreateBy(pen);
        }
    }
}
