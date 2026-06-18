namespace ClapInfra.ClapProfession
{
    public abstract class ClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        protected HashSet<string> _availableSkillNames = new();
        private readonly Dictionary<string, Func<TISkillContext, bool>> _skillChecker = new();
        private readonly Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> _skillSourceFileGenerator = new();

        public HashSet<string> AvailableSkillNames => _availableSkillNames;
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;

        protected ClapSkillPackage()
        {
            RegistSkills();
        }

        protected void RegistSkill(
            string skillName,
            Func<TISkillContext, bool> checker,
            Func<TISkillContext, TIDSLSourceFile> generator)
        {
            _availableSkillNames.Add(skillName);
            _skillChecker.Add(skillName, checker);
            _skillSourceFileGenerator.Add(skillName, generator);
        }
        protected virtual void RegistSkills() { }
        public virtual void RegistAnalyzers() { }
        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }
}
