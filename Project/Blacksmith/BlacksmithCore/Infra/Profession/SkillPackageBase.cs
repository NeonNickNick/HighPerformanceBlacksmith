using BlacksmithCore.Infra.DSL;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = BlacksmithDSL;

    public abstract class SkillPackageBase
    {
        protected HashSet<string> _availableSkillNames = new();
        private readonly Dictionary<string, Func<ISkillCheckContext, bool>> _skillChecker = new();
        private readonly Dictionary<string, Func<ISkillCheckContext, IDSLSourceFile>> _skillSourceFileGenerator = new();

        public HashSet<string> AvailableSkillNames => _availableSkillNames;
        public Dictionary<string, Func<ISkillCheckContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<ISkillCheckContext, IDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;

        protected SkillPackageBase()
        {
            RegistSkills();
        }

        protected void RegistSkill(
            string skillName,
            Func<ISkillCheckContext, bool> checker,
            Func<ISkillCheckContext, IDSLSourceFile> generator)
        {
            _availableSkillNames.Add(skillName);
            _skillChecker.Add(skillName, checker);
            _skillSourceFileGenerator.Add(skillName, generator);
        }
        protected virtual void RegistSkills() { }
        public virtual void RegistAnalyzers() { }

        public IDSLSourceFile PassiveSkill(ISkillCheckContext sc)
        {
            var sf = PassiveSkillImpl(sc);
            sf.IsPassive = true;
            return sf;
        }
        public virtual IDSLSourceFile PassiveSkillImpl(ISkillCheckContext sc)
        {
            return new DSL.SourceFile();
        }
        public abstract SkillPackageBase Copy();
    }
}
