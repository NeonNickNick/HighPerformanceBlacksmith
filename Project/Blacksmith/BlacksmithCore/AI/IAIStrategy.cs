using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.AI
{
    public interface IAIStrategy
    {
        string Name { get; }
        public void Init(GameInstance gameInstance);
        public SkillDeclareData ChooseSkill();
    }
}
