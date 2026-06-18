using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Models.Components.AnalyzableDatas
{
    public class AttackStage : BlacksmithEnum<AttackStage>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue OnHitArmorFirstTime() => GetCEValue();
        [IsBlacksmithEnumMember(1)]
        public CEValue OnHitBody() => GetCEValue();
        [IsBlacksmithEnumMember(2)]
        public CEValue OnEnd() => GetCEValue();
    }
    public class AttackAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required AttackType.CEValue Type { get; init; }
        public required int Power { get; set; }
        public required float APFactor { get; init; }
        public int TotalDamage { get; set; } = 0;
        public Dictionary<AttackStage.CEValue, List<string>> StageKeys { get; init; } = new();
        public Dictionary<string, float> ExtraParams { get; init; } = new();
    }
}