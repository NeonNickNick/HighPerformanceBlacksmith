using System.Text.RegularExpressions;
using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Profession
{
    public interface ISkillCheckContext
    {
        public ISudoOperations SudoOperations { get; }
        public Community Self { get; }
        public SkillDeclareData SkillDeclareData { get; }
    }
    public interface ISkillExecuteContext
    {
        public ISudoOperations SudoOperations { get; }
        public SkillDeclareData SkillDeclareData { get; }
    }
    public interface ISudoOperations
    {
        public GameInstance DeepCopy();
    }
}
