using BlacksmithCore.Infra.Attributes.Analyzer;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Models.Components.AnalyzableDatas
{
    [IsManual]
    public partial class DefenseAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required DefenseEntity Defense { get; init; } = null!;
        public DefenseAnalyzableData Copy()
        {
            return new()
            {
                AnalyzerKey = AnalyzerKey,
                Clock = Clock.Copy(),
                Defense = Defense.Copy(),
            };
        }
    }
}