using System.Reflection;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.Attributes.SkillMetadata.Core;
using BlacksmithCore.Infra.DSL;

namespace BlacksmithCore.Infra.Profession
{
    public static class ProfessionRegistry
    {
        public static readonly HashSet<string> Professions = new();
        private static HashSet<string> _mainProfessionSkillNames = null!;
        private static readonly object _mainProfessionSkillNamesLock = new();
        public static IReadOnlySet<string> MainProfessionSkillNames
        {
            get
            {
                if (_mainProfessionSkillNames == null)
                {
                    lock (_mainProfessionSkillNamesLock)
                    {
                        if (_mainProfessionSkillNames == null)
                        {
                            var set = new HashSet<string>();
                            foreach (var skillName in SkillMetadataDict.Keys)
                            {
                                foreach (var isc in SkillMetadataDict[skillName])
                                {
                                    if (isc.GetType() == typeof(IsProfessionSkill))
                                    {
                                        set.Add(skillName);
                                        break;
                                    }
                                }
                            }
                            _mainProfessionSkillNames = set;
                        }
                    }
                }
                return _mainProfessionSkillNames;
            }
        }
        public static readonly Dictionary<string, HashSet<ISkillMetadata>> SkillMetadataDict = new();
        private static readonly Dictionary<string, List<ProfessionModifier>> _modifierInstances = new();

        public static void RegistProfessionName(string professionName)
        {
            if (Professions.Contains(professionName))
            {
                throw new ArgumentException($"Profession \"{professionName}\" already exists! Expansion addition failed!");
            }
            Professions.Add(professionName);
            Console.WriteLine($"Successfully added the extended profession \"{professionName}\"!");
        }

        public static void CollectSkillMetadata(SkillPackageBase package)
        {
            static bool IsValidSkillMethod(MethodInfo method)
            {
                return method.IsPrivate
                    && method.ReturnType == typeof(IDSLSourceFile)
                    && method.GetParameters() is { Length: 1 } parameters
                    && parameters[0].ParameterType == typeof(ISkillContext);
            }
            var minfos = package.GetType().GetMethods(
                BindingFlags.NonPublic | BindingFlags.Static
            );

            foreach (var info in minfos)
            {
                if (!IsValidSkillMethod(info))
                {
                    continue;
                }

                // 直接覆盖
                SkillMetadataDict[info.Name.ToLower()] = new();
                var metadatas = info.GetCustomAttributes();
                if (metadatas == null)
                {
                    continue;
                }
                foreach (var m in metadatas)
                {
                    if (m is ISkillMetadata metadata)
                    {
                        SkillMetadataDict[info.Name.ToLower()].Add(metadata);
                    }
                }
            }
        }

        public static void RegistProfessionModifier(string targetName, ProfessionModifier modifier)
        {
            if (!_modifierInstances.TryGetValue(targetName, out var list))
            {
                _modifierInstances[targetName] = list = new();
            }
            list.Add(modifier);
        }

        public static void AddModOnInit(MainProfession package)
        {
            if (_modifierInstances.TryGetValue(package.GetType().Name, out var instances))
            {
                foreach (var instance in instances)
                {
                    var modifier = (ProfessionModifier)instance.Copy();
                    package.AvailableSkillNames.UnionWith(modifier.AvailableSkillNames);
                    foreach (var kv in modifier.SkillChecker)
                    {
                        package.SkillChecker[kv.Key] = kv.Value;
                    }
                    foreach (var kv in modifier.SkillSourceFileGenerator)
                    {
                        package.SkillSourceFileGenerator[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
