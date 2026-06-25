using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;

namespace ModExamples.MonkMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Clone : MainProfession
    {
        private ClapStateVar<int> _gbcTimes = new(0);
        public override IDSLSourceFile PassiveSkillImpl(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(a => _gbcTimes.Increment(), true)
                .WriteDefense(100f - 60f * _gbcTimes.Value, new PercentageReduction(baseline: 100));
            return DSL.CreateBy(pen);
        }
    }
}
