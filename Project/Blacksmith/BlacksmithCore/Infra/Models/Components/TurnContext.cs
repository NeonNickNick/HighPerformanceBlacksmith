using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;
using ClapInfra.ClapModels.Entities;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components
{
    public interface IAnalyzableData : IClapAnalyzableData<Community>
    {  
    }
    public class TurnContext : ClapTurnContext<IAnalyzableData, Community>, IComponent<Body>, IUpdatePerRound
    {
        private class Unit
        {
            public Action<IAnalyzableData> Action;
            public ClapRoundClock Clock;
            public Unit(Action<IAnalyzableData> action, ClapRoundClock clock)
            {
                Action = action;
                Clock = clock;
            }
        }
        private Dictionary<Type, List<Unit>> _preprocesses = new();
        public TurnContext() : base(new()
        {
            typeof(AttackAnalyzableData),
            typeof(DefenseAnalyzableData),
            typeof(ResourceAnalyzableData),
            typeof(EffectAnalyzableData)
        })
        {
            foreach (var key in _analyzableDataLists.Keys)
            {
                _preprocesses[key] = new();
            }
        }
        public void Copy(TurnContext origin)
        {
            foreach (var key in _analyzableDataLists.Keys)
            {//权宜之计
                _preprocesses[key].Clear();
            }
            var attack = Get<AttackAnalyzableData>();
            attack.Clear();
            foreach(var a in origin.Get<AttackAnalyzableData>())
            {
                attack.Add(new() 
                {
                    AnalyzerKey = a.AnalyzerKey,
                    Clock = a.Clock.Copy(),
                    Type = a.Type,
                    Power = a.Power,
                    APFactor = a.APFactor,
                    TotalDamage = a.TotalDamage,
                    StageKeys = a.StageKeys.ToDictionary(),
                    ExtraParams = a.ExtraParams.ToDictionary()
                });
            }

            var defense = Get<DefenseAnalyzableData>();
            defense.Clear();
            foreach (var a in origin.Get<DefenseAnalyzableData>())
            {
                defense.Add(new()
                {
                    AnalyzerKey = a.AnalyzerKey,
                    Clock = a.Clock.Copy(),
                    Defense = a.Defense,  //权宜之计 
                    Power = a.Power,
                });
            }

            var resource = Get<ResourceAnalyzableData>();
            resource.Clear();
            foreach (var a in origin.Get<ResourceAnalyzableData>())
            {
                resource.Add(new()
                {
                    AnalyzerKey = a.AnalyzerKey,
                    Clock = a.Clock.Copy(),
                    Type = a.Type,
                    Power = a.Power,
                });
            }

            var effect = Get<EffectAnalyzableData>();
            effect.Clear();
            foreach (var a in origin.Get<EffectAnalyzableData>())
            {
                effect.Add(new()
                {
                    AnalyzerKey = a.AnalyzerKey,
                    Clock = a.Clock.Copy(),
                    EntityClock = a.Clock.Copy(),
                    Type = a.Type,
                    Power = a.Power,
                    TargetType = a.TargetType
                });
            }
        }
        public void AddPreprocess<TAnalyzableData>(
            Action<TAnalyzableData> preprocess,
            int delayRounds = 0,
            int remainingRounds = 1,
            bool isInfinite = false)
            where TAnalyzableData : IAnalyzableData
        {
            var temp = (IAnalyzableData analyzableData) =>
            {
                preprocess((TAnalyzableData)analyzableData);
            };
            _preprocesses[typeof(TAnalyzableData)].Add(new(temp, new(remainingRounds: remainingRounds, delayRounds: delayRounds, isInfinite: isInfinite)));
        }
        public void Update()
        {
            foreach (var list in _preprocesses.Values)
            {
                int n = list.Count;
                for (int i = n - 1; i >= 0; i--)
                {
                    if (list[i].Clock.IsDead)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    list[i].Clock.RoundPass();
                }
            }
        }
        public override void WriteAnalyzableData(IAnalyzableData analyzableData)
        {
            var pp = _preprocesses[analyzableData.GetType()];
            pp.ForEach(p =>
            {
                if (p.Clock.IsRinging)
                {
                    p.Action(analyzableData);
                }
            });
            base.WriteAnalyzableData(analyzableData);
        }

        public List<(string name, int delayRounds, int power)> GetFutureDefenseView()
        {
            return Get<DefenseAnalyzableData>()
                .Select(d => (d.Defense.GetType().Name, d.Clock.DelayRounds, (int)d.Defense.Power))
                .ToList();
        }
        public List<(string name, int delayRounds, int power)> GetFutureAttackView()
        {
            return Get<AttackAnalyzableData>()
                .Select(a => (a.Type.ToString(), a.Clock.DelayRounds, (int)a.Power))
                .ToList();
        }
    }
}