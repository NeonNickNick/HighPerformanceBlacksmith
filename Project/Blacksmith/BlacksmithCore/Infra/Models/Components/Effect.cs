using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
namespace BlacksmithCore.Infra.Models.Components
{
    public class Effect : IUpdatePerRound, IComponent<Body>
    {
        private readonly List<EffectEntity> _effects = new();
        public List<EffectEntity> Effects => _effects;
        public void Copy(Effect origin)
        {
            _effects.Clear();
            foreach (var effect in origin._effects)
            {
                _effects.Add(effect.Copy());
            }
        }
        public void Add(EffectEntity effectEntity)
        {
            _effects.Add(effectEntity);
        }
        public void AddRange(List<EffectEntity> effectEntities)
        {
            _effects.AddRange(effectEntities);
        }
        public IEnumerable<EffectEntity> Where(EffectType.CEValue type)
        {
            return _effects.Where(e => e.Type == type);
        }
        public void Update()
        {
            _effects.ForEach(e => e.Clock.RoundPass());
            _effects.RemoveAll(e => e.Clock.IsDead);
        }
        public List<EffectEntity> Get()
        {
            return _effects;
        }
    }
}
