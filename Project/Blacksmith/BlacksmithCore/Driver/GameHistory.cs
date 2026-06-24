using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Driver
{
    public class GameHistory
    {
        public List<(SkillDeclareData, SkillDeclareData)> SkillHistory { get; set; } = new();
        public void Copy(GameHistory origin)
        {
            SkillHistory = new(origin.SkillHistory);
        }
    }
}
