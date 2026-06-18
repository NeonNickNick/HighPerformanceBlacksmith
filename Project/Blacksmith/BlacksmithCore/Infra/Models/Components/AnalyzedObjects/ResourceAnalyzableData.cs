using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Models.Components.AnalyzableDatas
{
    public class ResourceAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required ResourceType.CEValue Type { get; init; }
        public required float Power { get; set; }
    }
}
