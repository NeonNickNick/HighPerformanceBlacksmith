using BlacksmithCore.Infra.Attributes.SkillMarkOnly;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;

namespace ModExamples.PhantomBookMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsExperimental]
    public partial class Nightmare : MainProfession
    {
        private bool DreamDiveCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile DreamDive(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .WriteAttack(2f, AttackType.Instance.Real())
                .WriteDefense(5f, new CommonReduction())
                .WriteRecovery(1)
                .WriteResource(1f, ResourceType.Instance.Spirit())
                .WriteDefense(0f, new MagicalImmunity());
            return DSL.CreateBy(pen);
        }
        private bool MaterializeCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 1f)
                && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Spirit(), 2f);
        }
        [HasAttack]
        private IDSLSourceFile Materialize(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .UseResource(2f, ResourceType.Instance.Spirit())
                .WriteAttack(4f, AttackType.Instance.Physical())
                .WriteAttack(4f, AttackType.Instance.Real())
                .WriteDefense(4f, new CommonReduction());
            return DSL.CreateBy(pen);
        }
        private bool ClingingHauntCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Spirit(), 1f);
        }
        [HasAttack]
        private IDSLSourceFile ClingingHaunt(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Spirit())
                .WriteAttack(2f, AttackType.Instance.Magical(), delayRounds: 0)
                .WriteAttack(2f, AttackType.Instance.Magical(), delayRounds: 1)
                .WriteAttack(1f, AttackType.Instance.Magical(), delayRounds: 2);
            return DSL.CreateBy(pen);
        }
        private bool ChannelingCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Spirit(), 1f);
        }
        private IDSLSourceFile Channeling(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Spirit())
                .WriteRecovery(3)
                .WriteDefense(2f, new CommonReduction())
                .WriteDefense(0f, new MagicalImmunity());
            return DSL.CreateBy(pen);
        }
    }
}
