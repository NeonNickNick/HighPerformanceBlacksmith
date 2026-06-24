using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Profession;


namespace BlacksmithCore.AI.Strategies
{
    public class MetadataStrategy : IAIStrategy
    {
        private GameInstance _main = null!;
        private static Random _random = new();

        public string Name => "Metadata";

        public MetadataStrategy()
        {
        }

        public void Init(GameInstance gameInstance)
        {
            _main = gameInstance;
        }

        public SkillDeclareData ChooseSkill()
        {
            var set = new HashSet<SkillDeclareData>();
            var names = _main.Enemy.Focus.Get<Skill>().GetAvailableSkillNames();
            foreach (var n in names)
            {
                if (ProfessionRegistry.SkillMetadataDict[n].FirstOrDefault(s => s is IsInfinite _) != null)
                {
                    int layer = 1;
                    while (_main.ETryDeclare(SkillDeclareData.Parse($"{n}(p:{layer})")!) == SkillDeclareResult.Success)
                    {
                        layer++;
                    }
                    layer--;
                    set.Add(SkillDeclareData.Parse($"{n}(p:{layer})")!);
                    if (layer > 1)
                    {
                        layer--;
                        set.Add(SkillDeclareData.Parse($"{n}(p:{layer})")!);
                    }
                }
                else
                {
                    if (_main.ETryDeclare(SkillDeclareData.Parse(n)!) == SkillDeclareResult.Success)
                    {
                        set.Add(SkillDeclareData.Parse(n)!);
                    }
                }

            }
            Dictionary<Labels, string> dict = new();
            foreach (var s in set)
            {
                dict[(Labels)ProfessionRegistry.SkillMetadataDict[s.SkillName].FirstOrDefault(m => m is Labels _)!] = s.SkillName;
            }

            var ls = dict.Keys.ToList();
            for (int i = ls.Count - 1; i >= 0; --i)
            {
                if (ProfessionRegistry.SkillMetadataDict[dict[ls[i]]].FirstOrDefault(m => m is HasAttack _) == null)
                {
                    if (_random.NextDouble() < 0.7)
                    {
                        dict.Remove(ls[i]);
                    }
                }
                if (ls[i].Impression == Impression.Aggressive)
                {
                    if (_random.NextDouble() < 0.5)
                    {
                        dict.Remove(ls[i]);
                    }
                }
                if (ls[i].Impression == Impression.Conservative)
                {
                    if (_random.NextDouble() < 0.4)
                    {
                        dict.Remove(ls[i]);
                    }
                }
            }

            var name = "iron";

            var ordinarys = dict.Keys.Where(k => k.Strength == Strength.Ordinary).ToList();
            if (ordinarys.Count > 0 && _random.NextDouble() < 0.3)
            {
                name = dict[ordinarys[_random.Next(ordinarys.Count)]];
            }

            var strongs = dict.Keys.Where(k => k.Strength == Strength.Strong).ToList();
            if (strongs.Count > 0 && _random.NextDouble() < 0.9)
            {
                name = dict[strongs[_random.Next(strongs.Count)]];
            }

            var supers = dict.Keys.Where(k => k.Strength == Strength.Super).ToList();
            if (supers.Count > 0 && _random.NextDouble() < 0.8)
            {
                name = dict[supers[_random.Next(supers.Count)]];
            }

            var all = set.Where(s => s.SkillName == name).ToList();
            var chosen = all[_random.Next(all.Count)];
            return chosen;
        }
    }
}
