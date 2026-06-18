using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;
namespace BlacksmithCore.Infra.Models.Components.AnalyzedObjects
{
    public abstract class DefenseEntity : IAnalyzableData
    {
        public abstract string AnalyzerKey { get; init; }
        public abstract DefenseType.CEValue Type { get; init; }
        public abstract int Power { get; set; }
        public abstract ClapRoundClock Clock { get; init; }
        public virtual bool CanMerge { get; init; } = false;
        public virtual void Merge(DefenseEntity addition)
        {
        }
        public virtual void Update()
        {
            Clock.RoundPass();
            if (Power <= 0)
            {
                Clock.Kill();
            }
        }
    }
}