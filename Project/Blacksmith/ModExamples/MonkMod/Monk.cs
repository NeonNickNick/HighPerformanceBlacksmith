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

namespace ModExamples.MonkMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Monk : MainProfession
    {
        private List<Body> _clones = new();
        private ClapStateVar<int> _cloneNum = new(0);
        private ClapStateVar<int> _gbcTimes = new(0);
        private ClapStateVar<float> _transmitPercent = new(0.5f);
        private ClapStateVar<bool> _mist = new(true);
        private bool JadeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Jade(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Jade());
            return DSL.CreateBy(pen);
        }
        private bool GhostStepCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Jade(), 1f)
                && _cloneNum.Value < 2;
        }
        private IDSLSourceFile GhostStep(ISkillCheckContext sc)
        {
            Body clone = null!;
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Jade())
                .LoseHP(1)
                .WriteFree(source =>
                {
                    _cloneNum.Increment();
                    clone = new Body(source, $"Clone{_cloneNum.Value}");
                    clone.Get<Health>().MHP = source.Focus.Get<Health>().MHP;
                    clone.Get<Health>().HP = source.Focus.Get<Health>().HP;
                    clone.Get<Skill>().AddPackage(new(new Clone()));
                    _clones.Add(clone);
                    source.AddTransform(() =>
                    {
                        source.SummonList.Add(clone);
                    });
                    source.AddCallbackKilled(clone, () =>
                    {
                        _cloneNum.Decrement();
                        DSL.Create(source, sf => sf
                            .WriteAttack(3f, AttackType.Instance.Physical())
                            .WriteAttack(2f, AttackType.Instance.Magical()))
                            .Compile().Execute(source);
                    });
                }, true)
                .RegistCallbackOnJudge(
                    new()
                    {
                        new ModifierCallback((player, enemy) =>
                        {
                            foreach(var analyzableData in player.Focus.Get<TurnContext>().Get<AttackAnalyzableData>())
                            {
                                if(analyzableData.Clock.IsRinging)
                                {
                                    analyzableData.AddStage(AttackStage.Instance.OnHitBody(), (community, body, aanalyzableData) =>
                                    {
                                        if (clone.Get<Health>().IsKilled)
                                        {
                                            return;
                                        }
                                        int transmit = (int)MathF.Ceiling(aanalyzableData.Power * _transmitPercent.Value);
                                        clone.Get<Health>().LoseHP(transmit);
                                        aanalyzableData.Power -= transmit;
                                    });
                                }
                            }
                        },
                        JudgeStage.Instance.OnApplyingOthers(),
                        ModifierOrder.Before,
                        new(isInfinite: true, forceKill: () =>
                        {
                            return clone.Get<Health>().IsKilled;
                        })),
                    });
            return DSL.CreateBy(pen);
        }
        private bool GoldenBellCoverCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Jade(), 1f);
        }
        private IDSLSourceFile GoldenBellCover(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Jade())
                .WriteFree(a => _gbcTimes.Increment(), true)
                .WriteDefense(100f - 60f * _gbcTimes.Value, new PercentageReduction(baseline: 100));
            return DSL.CreateBy(pen);
        }
        private bool MazeFistCheck(ISkillCheckContext sc)
        {
            return _cloneNum.Value > 0;
        }
        [HasAttack]
        private IDSLSourceFile MazeFist(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    _clones.RemoveAll(c => c.Get<Health>().IsKilled);
                    var hpMax = _clones.MaxBy(c => c.Get<Health>().HP);
                    hpMax!.Get<Health>().LoseMHP(114514);
                }, true)
                .WriteAttack(6f, AttackType.Instance.Physical())
                    .WithFree(
                        AttackStage.Instance.OnHitArmorFirstTime(),
                        (source, target, aanalyzableData) =>
                        {
                            var entity = new EffectEntity(
                                EffectType.Instance.AfterTransport(),
                                0,
                                new(delayRounds: 1, remainingRounds: 1));
                            entity.Execute = (body) =>
                            {
                                body.Get<TurnContext>().Get<AttackAnalyzableData>()
                                    .RemoveAll(a => a.Clock.IsRinging);
                            };
                            source.Focus.Get<Effect>().Add(entity);
                        });
            return DSL.CreateBy(pen);
        }
        private bool MistCheck(ISkillCheckContext sc)
        {
            return _mist.Value
                && _cloneNum.Value > 0
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Mist(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    _transmitPercent.Set(0.9f);
                    _mist.Set(false);
                    _clones.RemoveAll(c => c.Get<Health>().IsKilled);
                    var hpMin = _clones.MinBy(c => c.Get<Health>().HP);
                    var entity = new EffectEntity(
                        EffectType.Instance.AfterAnalyzableDataWritten(),
                        3f,
                        new(remainingRounds: 3));
                    entity.Execute = (body) =>
                    {
                        body.Get<Health>().GainHP((int)entity.Power);
                    };
                    hpMin!.Get<Effect>().Add(entity);
                }, true)
                .RegistCallbackOnJudge(new()
                {
                    new ModifierCallback((player, enemy) =>
                    {
                        _transmitPercent.Reset();
                        _mist.Reset();
                    },
                    JudgeStage.Instance.OnEnd(),
                    ModifierOrder.Before,
                    new(delayRounds: 2, remainingRounds: 1))
                });
            return DSL.CreateBy(pen);
        }
        private bool DisillusionmentCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Jade(), 2f);
        }
        [HasAttack]
        private IDSLSourceFile Disillusionment(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2f, ResourceType.Instance.Jade())
                .WriteAttack(6f, AttackType.Instance.Physical())
                .WriteAttack(4f, AttackType.Instance.Magical());
            return DSL.CreateBy(pen);
        }
    }
}
