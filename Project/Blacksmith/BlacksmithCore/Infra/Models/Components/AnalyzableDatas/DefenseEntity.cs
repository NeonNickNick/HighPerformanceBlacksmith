using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;
namespace BlacksmithCore.Infra.Models.Components.AnalyzedObjects
{
    public partial class DefenseEntity : IAnalyzableData
    {
        public required string Name { get; init; }
        public required string AnalyzerKey { get; init; }
        public string MergeKey { get; init; } = nameof(StandardAnalyzers.DefaultMerge);
        public required DefenseType.CEValue Type { get; init; }
        public required int Power { get; set; }
        public required ClapRoundClock Clock { get; init; }
        public bool CanMerge { get; init; } = false;
        public void Update()
        {
            Clock.RoundPass();
            if (Power <= 0)
            {
                Clock.Kill();
            }
        }

    }
}