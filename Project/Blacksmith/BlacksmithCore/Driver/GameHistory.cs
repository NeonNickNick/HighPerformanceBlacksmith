namespace BlacksmithCore.Driver
{
    public class GameHistory
    {
        public List<((string SkillName, int Param, string StringParam), (string SkillName, int Param, string StringParam))> SkillHistory { get; set; } = new();
        public void Copy(GameHistory origin)
        {
            SkillHistory = new(origin.SkillHistory);
        }
    }
}
