using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;
using BlacksmithCore.Specific.BuiltInProfessions;

namespace BlacksmithCore.Infra.Models.Components
{
    public enum SkillDeclareResult
    {
        Success,
        Illegal,
        Rejected
    }
    public class PackageContainer
    {
        public string Name { get; }
        public MainProfession SkillPackage { get; }
        public ClapSharedFlag Flag { get; set; } = new();
        public PackageContainer(MainProfession skillpackage)
        {
            Name = skillpackage.GetType().Name;
            SkillPackage = skillpackage;
        }
        public PackageContainer Copy()
        {
            return new((MainProfession)SkillPackage.Copy())
            {
                Flag = Flag.Copy()
            };
        }
    }
    public class Skill :
        IComponent<Body>
    {
        protected List<PackageContainer> _packages = new() { new(new Common()) };
        public bool HaveProfession => _packages.Count > 1;
        public virtual void AddPackage(PackageContainer package)
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
        public virtual SkillDeclareResult TryDeclare(string skillName, ISkillCheckContext sc)
        {
            foreach (var package in _packages.Where(p => p.Flag.IsActive))
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
        public virtual IDSLSourceFile Declare(string skillName, ISkillExecuteContext sc)
        {
            foreach (var package in _packages.Where(p => p.Flag.IsActive))
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
                .Where(p => p.Flag.IsActive)
                .SelectMany(p => p.SkillPackage.AvailableSkillNames)
                .ToList();
        }
        public virtual List<string> GetActivePackageNames()
        {
            return _packages
                .Where(p => p.Flag.IsActive)
                .Select(p => p.Name)
                .ToList();
        }
        public void Copy(Skill origin)
        {
            _packages.Clear();
            foreach (var pc in origin._packages)
            {
                _packages.Add(pc.Copy());
            }
        }
        public HashSet<string> DisableAll()
        {
            var res = new HashSet<string>();
            foreach (var package in _packages)
            {
                package.Flag.Disable();
                res.Add(package.Name);
            }
            return res;
        }
        public void Enable(HashSet<string> names)
        {
            foreach (var package in _packages)
            {
                if (names.Contains(package.Name))
                {
                    package.Flag.Enable();
                }
            }
        }
        public List<IDSLSourceFile> GetPassiveSkill(ISkillExecuteContext sc)
        {
            return _packages.Where(p => p.Flag.IsActive).Select(p => p.SkillPackage.PassiveSkill(sc)).ToList();
        }
        public List<string> GetView()
        {
            if (_packages.Count < 2)
            {
                return new();
            }
            var temp = _packages.Select(p => p.Name).ToList();
            temp.RemoveAt(0);
            return temp;
        }
    }
}
