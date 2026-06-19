using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Models.Components.AnalyzableDatas
{
    public partial class EffectAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required ClapRoundClock EntityClock { get; init; }
        public required EffectType.CEValue Type { get; init; }
        public required EffectTargetType.CEValue TargetType { get; init; }
        public float Power { get; set; }
    }
}