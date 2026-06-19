using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Utils;

namespace BlacksmithCore.Infra.Judgement
{
    public partial class OverrideCallback : ICallbackOnJudge
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required JudgeStage.CEValue Stage { get; init; }
        public required bool IsPlayer { get; init; }
    }
}
