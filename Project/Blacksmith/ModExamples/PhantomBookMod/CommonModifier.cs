using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions;

namespace ModExamples.PhantomBookMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsProfessionModifier(nameof(Common))]
    public partial class CommonModifier : ProfessionModifier
    {
        private bool PhantomBookCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2.5f);
        }
        [IsProfessionSkill]
        private IDSLSourceFile PhantomBook(ISkillExecuteContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new PhantomBook()));
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    Common.ExcludeAllProfessions(source);
                }, false);
            return DSL.CreateBy(pen);
        }
    }
}
