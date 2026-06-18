using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Specific.Defense;
using ClapInfra.ClapModels.Entities;

namespace BlacksmithCore.Infra.Models.Components
{
    public class Defense : IUpdatePerRound, IComponent<Body>
    {
        private List<DefenseEntity> _defenses = new();
        public IReadOnlyList<DefenseEntity> Defenses => _defenses;
        public void Copy(Defense origin)
        {
            _defenses.Clear();
            foreach(var defense in origin._defenses)
            {
                //权宜之计
                _defenses.Add(new CommonReduction()
                {
                    AnalyzerKey = defense.AnalyzerKey,
                    Type = defense.Type,
                    Power = defense.Power,
                    Clock = defense.Clock.Copy(),
                    CanMerge = defense.CanMerge,
                });
            }
        }
        public void Update()
        {
            int n = _defenses.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                _defenses[i].Update();
                if (_defenses[i].Clock.IsDead)
                {
                    _defenses.RemoveAt(i);
                }
            }
        }
        public void Add(DefenseEntity addition)
        {
            if (Merge(addition))
            {
                return;
            }
            _defenses.Add(addition);
            _defenses = _defenses.OrderBy(d => d.Type).ToList();
        }
        private bool Merge(DefenseEntity addition)
        {
            if (!addition.CanMerge)
            {
                return false;
            }
            DefenseEntity? firstMatch = _defenses.Find(d => d.Type == addition.Type);
            if (firstMatch == null)
            {
                return false;
            }
            firstMatch.Merge(addition);
            return true;
        }
        public void Init()
        {
            _defenses.Clear();
        }
        public List<(string name, int power)> GetView()
        {
            return _defenses.Select(d => (d.GetType().Name, (int)d.Power)).ToList();
        }
    }
}