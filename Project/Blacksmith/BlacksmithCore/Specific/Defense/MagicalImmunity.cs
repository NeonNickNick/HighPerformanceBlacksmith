using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Specific.Defense
{
    public class MagicalImmunity : DefenseEntity
    {
        public override string AnalyzerKey { get; init; } = nameof(StandardAnalyzers.MagicalImmunity);
        public override DefenseType.CEValue Type { get; init; } = DefenseType.Instance.MagicalImmunity();
        public override int Power { get; set; } = 0;
        public override ClapRoundClock Clock { get; init; } = new();
    }
}
