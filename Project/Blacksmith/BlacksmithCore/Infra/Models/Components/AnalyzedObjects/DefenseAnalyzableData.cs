using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Models.Components.AnalyzableDatas
{
    public class DefenseAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required DefenseEntity Defense { get; init; } = null!;
        public int Power { get; set; }
    }
}