using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.DSL
{
    public interface IDSLSourceFile
    {
        public bool IsPassive { get; set; }
        public Intent Compile(JudgeRuleManager? judgeRuleManager = null);
        public void Move(Community newOwner, HashSet<DSLforSkillLogic.SentenceType> filter);
    }
}
