using BlacksmithCore.Infra.Attributes.SkillMarkOnly;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using ModExamples.HolyBookMod;
using ModExamples.HolyBookMod.Defense;
namespace ModExamples
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsExperimental]
    public partial class HolyBook : MainProfession
    {
        private bool CrossCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private IDSLSourceFile Cross(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Cross());
            return DSL.CreateBy(pen);
        }
        private bool PrayCheck(ISkillCheckContext sc) => true;
        private IDSLSourceFile Pray(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(3, new CommonReduction());
            return DSL.CreateBy(pen);
        }
        private bool ArkCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Cross(), 2f);
        }
        [HasAttack]
        private IDSLSourceFile Ark(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Cross())
                .WriteAttack(8, AttackType.Instance.Physical())
                .WriteDefense(1, new PercentageReduction(baseline: 2));
            return DSL.CreateBy(pen);
        }
        private int _blasphemyCount = 0;
        private bool BlasphemyCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Cross(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile Blasphemy(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Cross())
                .WriteAttack(2, AttackType.Instance.Real())
                .WriteDefense(2 + (int)MathF.Ceiling(_blasphemyCount / 3), new GreyHP())
                .WriteFree(a => _blasphemyCount++, true);
            return DSL.CreateBy(pen);
        }
        private bool RebirthCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Cross(), 1f);
        }
        private IDSLSourceFile Rebirth(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Cross())
                .WriteEffect(EffectType.Instance.AfterAnalyzableDataWritten()
                            , EffectTargetType.Instance.Self()
                            , 3f
                            , 3
                            , (Community source, Body target, EffectEntity effectEntity) =>
                            {
                                target.Get<Health>().GainHP((int)effectEntity.Power);
                            })
                .WriteDefense(1, new PercentageReduction(baseline: 4), delayRounds: 0)
                .WriteDefense(1, new PercentageReduction(baseline: 4), delayRounds: 1)
                .WriteDefense(1, new PercentageReduction(baseline: 4), delayRounds: 2);
            return DSL.CreateBy(pen);
        }
        private bool ExonerationCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Cross(), 1f);
        }
        private IDSLSourceFile Exoneration(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Cross())
                .WriteDefense(1, new PermanentRealReduction());
            return DSL.CreateBy(pen);
        }

    }
}
