using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using ModExamples.ProphetMod.Defense;

namespace ModExamples.ProphetMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Prophet : MainProfession
    {
        private Action<Community, Body, EffectEntity> _afterDodge = (source, b, c) =>
        {
            source.Focus.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
        };
        private Action<Community, Body, EffectEntity> _dodgeFail = (a, b, c) => { };
        private Pen Dodge => sf => sf
                .WriteEffect(
                    EffectType.Instance.AfterTransport(),
                    EffectTargetType.Instance.Self(),
                    power: 0f,
                    duration: 1,
                    (source, target, entity) =>
                    {
                        var alist = target.Get<TurnContext>().Get<AttackAnalyzableData>();
                        bool hasAtk = alist.Find(a => a.Clock.IsRinging) != null;
                        target.Get<TurnContext>().Get<AttackAnalyzableData>()
                              .RemoveAll(a => a.Clock.IsRinging);
                        if (hasAtk)
                        {
                            _afterDodge(source, target, entity);
                            _afterDodge = (source, b, c) =>
                            {
                                source.Focus.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
                            };
                        }
                        else
                        {
                            _dodgeFail(source, target, entity);
                            _dodgeFail = (a, b, c) => { };
                        }
                    }
                );
        private bool CrystalCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
        }
        private IDSLSourceFile Crystal(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Crystal());
            return DSL.CreateBy(pen);
        }
        private bool CrystalBallCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Crystal(), 2f)
                && !sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f);
        }
        private IDSLSourceFile CrystalBall(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2f, ResourceType.Instance.Crystal())
                .WriteResource(1f, ResourceType.Instance.CrystalBall());
            return DSL.CreateBy(pen);
        }
        private bool ForetoldCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Foretold(ISkillCheckContext sc)
        {
            Pen pen = (sf => sf
                .UseResource(1f, ResourceType.Instance.Iron()))
                + Dodge;
            return DSL.CreateBy(pen);
        }
        private bool GreatestCautionCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Crystal(), 1f);
        }
        private IDSLSourceFile GreatestCaution(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Crystal())
                .WriteFree(a =>
                {
                    _afterDodge += (Community source, Body target, EffectEntity entity) =>
                    {
                        DSL.Create(source, sf => sf.WriteAttack(4f, AttackType.Instance.Real(), delayRounds: 1)).Compile().Execute(source);
                    };
                }, true);
            return DSL.CreateBy(pen);
        }
        private bool RevelationCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        private IDSLSourceFile Revelation(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Iron())
                .WriteFree(a =>
                {
                    _afterDodge += (Community source, Body target, EffectEntity entity) =>
                    {
                        source.Focus.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
                    };
                }, true);
            return DSL.CreateBy(pen);
        }
        private bool AssertionCheck(ISkillCheckContext sc) => true;
        private IDSLSourceFile Assertion(ISkillCheckContext sc)
        {
            Pen pen = (sf => sf
                .WriteFree(a =>
                {
                    _dodgeFail += (Community source, Body target, EffectEntity entity) =>
                    {
                        source.Focus.Get<Health>().LoseHP(2);
                    };
                }, true))
                + Dodge;
            return DSL.CreateBy(pen);
        }
        private bool CrystalWallCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f);
        }
        private IDSLSourceFile CrystalWall(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.CrystalBall())
                .WriteDefense(7, new CrystalWall(), delayRounds: 0)
                .WriteDefense(7, new CrystalWall(), delayRounds: 1);
            return DSL.CreateBy(pen);
        }
        private bool RefractionCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f);
        }
        private IDSLSourceFile Refraction(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(1, new PercentageReduction(baseline: 2));
            return DSL.CreateBy(pen);
        }
        private bool UltimatumCheck(ISkillCheckContext sc)
        {
            return sc.SkillDeclareData.Param > 5
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.CrystalBall(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Crystal(), 2f);
        }
        [HasAttack]
        private IDSLSourceFile Ultimatum(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.CrystalBall())
                .UseResource(2f, ResourceType.Instance.Crystal())
                .WriteAttack(12f, AttackType.Instance.Real(), delayRounds: sc.SkillDeclareData.Param);
            return DSL.CreateBy(pen);
        }
    }
}
