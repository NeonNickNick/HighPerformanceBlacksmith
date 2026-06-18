using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;
namespace BlacksmithCore.Specific.Defense
{
    public class RealReduction : DefenseEntity
    {
        public override string AnalyzerKey { get; init; } = nameof(StandardAnalyzers.DefaultReduction);
        public override DefenseType.CEValue Type { get; init; } = DefenseType.Instance.RealReduction();
        public override int Power { get; set; } = 0;
        public override ClapRoundClock Clock { get; init; } = new();
    }
}