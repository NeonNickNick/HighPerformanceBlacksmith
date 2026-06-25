using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions;

namespace ModExamples.CauldronMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsProfessionModifier(nameof(Common))]
    public partial class CommonModifier : ProfessionModifier
    {
        private bool CauldronCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3f);
        }
        [IsEquipmentSkill]
        private IDSLSourceFile Cauldron(ISkillExecuteContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Cauldron()));
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(Common), nameof(Cauldron));
                }, false);
            return DSL.CreateBy(pen);
        }
    }
}
