using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Profession
{
    public interface ISkillContext
    {
        public ISudoOperations SudoOperations { get; }
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
        public string StringParam { get; }
        public (string SkillName, int Param, string StringParam) History { get; }
    }
    public interface ISudoOperations
    {
        public GameInstance DeepCopy();
        public IReadOnlyList<((string SkillName, int Param, string StringParam), (string SkillName, int Param, string StringParam))> SkillHistory { get; }
        public ICompileTimeMetadata CompileTimeMetadata { get; }
    }
    public interface ICompileTimeMetadata
    {
        public IReadOnlySet<string> MainProfessionSkillNames { get; }
        public IReadOnlySet<string> EquipmentSkillNames { get; }
    }
}
