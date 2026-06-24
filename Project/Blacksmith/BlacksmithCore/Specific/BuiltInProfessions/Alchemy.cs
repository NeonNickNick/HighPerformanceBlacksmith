using BlacksmithCore.Infra.Attributes.Analyzer;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Alchemy : MainProfession
    {
        private static bool MidasTouchCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1, true);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile MidasTouch(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron(), true)
                .WriteResource(5, ResourceType.Instance.Gold_Iron());
            return DSL.CreateBy(pen);
        }
    }
}