using BlacksmithCore.Infra.DSL;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;

    public abstract class SkillPackageBase
    {
        protected HashSet<string> _availableSkillNames = new();
        private readonly Dictionary<string, Func<ISkillContext, bool>> _skillChecker = new();
        private readonly Dictionary<string, Func<ISkillContext, IDSLSourceFile>> _skillSourceFileGenerator = new();

        public HashSet<string> AvailableSkillNames => _availableSkillNames;
        public Dictionary<string, Func<ISkillContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<ISkillContext, IDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;

        protected SkillPackageBase()
        {
            RegistSkills();
        }

        protected void RegistSkill(
            string skillName,
            Func<ISkillContext, bool> checker,
            Func<ISkillContext, IDSLSourceFile> generator)
        {
            _availableSkillNames.Add(skillName);
            _skillChecker.Add(skillName, checker);
            _skillSourceFileGenerator.Add(skillName, generator);
        }
        protected virtual void RegistSkills() { }
        public virtual void RegistAnalyzers() { }

        public IDSLSourceFile PassiveSkill(ISkillContext sc)
        {
            var sf = PassiveSkillImpl(sc);
            sf.IsPassive = true;
            return sf;
        }
        public virtual IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
        {
            return new DSL.SourceFile(sc.Self);
        }
        public abstract SkillPackageBase Copy();
    }
}
