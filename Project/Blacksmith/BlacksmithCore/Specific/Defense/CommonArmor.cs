using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;
namespace BlacksmithCore.Specific.Defense
{
    public class CommonArmor : DefenseEntity
    {
        public override string AnalyzerKey { get; init; } = nameof(StandardAnalyzers.DefaultArmor);
        public override DefenseType.CEValue Type { get; init; } = DefenseType.Instance.CommonArmor();
        public override int Power { get; set; } = 0;
        public override ClapRoundClock Clock { get; init; } = new(isInfinite: true);
        public override bool CanMerge { get; init; } = false;
    }
}