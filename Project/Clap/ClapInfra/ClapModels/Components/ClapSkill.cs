using ClapInfra.ClapProfession;

namespace ClapInfra.ClapModels.Components
{
    public enum SkillDeclareResult
    {
        Success,
        Illegal,
        Rejected
    }
    public abstract class ClapPackageContainer<TMainProfession>
        where TMainProfession : notnull
    {
        public string Name { get; }
        public TMainProfession SkillPackage { get; }
        public ClapPackageContainer(TMainProfession skillpackage)
        {
            Name = skillpackage.GetType().Name;
            SkillPackage = skillpackage;
        }
    }
    public abstract class ClapSkill<TClapPackageContainer, TMainProfession, TISkillContext, TIDSLSourceFile>
        where TClapPackageContainer : ClapPackageContainer<TMainProfession>
        where TMainProfession : ClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        protected abstract List<TClapPackageContainer> _packages { get; set; }
        public virtual void AddPackage(TClapPackageContainer package)
        {
            _packages.Add(package);
        }
        public virtual void RemovePackage(string name)
        {
            _packages.RemoveAll(p => p.Name == name);
        }
        public virtual void AddSkill(string packageName, string skillName)
        {
            _packages.Find(p => p.Name == packageName)?.SkillPackage.AvailableSkillNames.Add(skillName.ToLower());
        }
        public virtual void RemoveSkill(string packageName, string skillName)
        {
            _packages.Find(p => p.Name == packageName)?.SkillPackage.AvailableSkillNames.Remove(skillName.ToLower());
        }
        public virtual SkillDeclareResult TryDeclare(string skillName, TISkillContext sc)
        {
            foreach (var package in _packages)
            {
                if (!package.SkillPackage.AvailableSkillNames.Contains(skillName))
                {
                    continue;
                }
                if (package.SkillPackage.SkillChecker[skillName](sc))
                {
                    return SkillDeclareResult.Success;
                }
                else
                {
                    return SkillDeclareResult.Rejected;
                }
            }
            return SkillDeclareResult.Illegal;
        }
        public virtual TIDSLSourceFile Declare(string skillName, TISkillContext sc)
        {
            foreach (var package in _packages)
            {
                if (!package.SkillPackage.AvailableSkillNames.Contains(skillName))
                {
                    continue;
                }
                return package.SkillPackage.SkillSourceFileGenerator[skillName](sc);
            }
            throw new ArgumentException("Unreachable1!");
        }
        public virtual List<string> GetAvailableSkillNames()
        {
            return _packages
                .SelectMany(p => p.SkillPackage.AvailableSkillNames)
                .ToList();
        }
        public virtual List<string> GetActivePackageNames()
        {
            return _packages
                .Select(p => p.Name)
                .ToList();
        }
    }
}
