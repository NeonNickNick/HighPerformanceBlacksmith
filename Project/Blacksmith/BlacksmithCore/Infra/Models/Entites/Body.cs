using BlacksmithCore.Infra.Models.Components;

namespace BlacksmithCore.Infra.Models.Entites
{
    public interface IUpdatePerRound
    {
        public void Update();
    }
    public interface IComponent<TBody>
    {

    }
    public class Body
    {
        private string _name;
        public Community Community { get; }
        protected Dictionary<Type, IComponent<Body>> _components = new();

        public Body(Community community, string name)
        {
            _name = name;
            Community = community;
            Add(new HashSet<IComponent<Body>>()
        {
            new Skill(),
            new Health(),
            new Defense(),
            new Resource(),
            new Effect(),
            new TurnContext()
        });
        }

        protected void Add(HashSet<IComponent<Body>> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }
            foreach (var obj in components)
            {
                if (obj == null)
                {
                    throw new ArgumentException("Body component cannot be null!");
                }
                if (obj is ValueType)
                {
                    throw new ArgumentException(
                        $"Cannot add value type {obj.GetType()} as component");
                }
                _components[obj.GetType()] = obj;//添加重复组件默认行为是直接覆盖
            }
        }
        public TTargetComponent Get<TTargetComponent>()
            where TTargetComponent : IComponent<Body>
        {
            if (_components.TryGetValue(typeof(TTargetComponent), out var value))
            {
                return (TTargetComponent)value;
            }
            else
            {
                throw new ArgumentException($"Cannot find component {nameof(TTargetComponent)}!");
            }
        }
        public void Update()
        {
            foreach (var component in _components.Values)
            {
                if (component is IUpdatePerRound updateComponent)
                {
                    updateComponent.Update();
                }
            }
        }
        public void Copy(Body origin)
        {
            Get<Skill>().Copy(origin.Get<Skill>());
            Get<Health>().Copy(origin.Get<Health>());
            Get<Defense>().Copy(origin.Get<Defense>());
            Get<Resource>().Copy(origin.Get<Resource>());
            Get<Effect>().Copy(origin.Get<Effect>());
            Get<TurnContext>().Copy(origin.Get<TurnContext>());
        }
        public BodyView GetView()
        {
            return new()
            {
                BodyName = _name,
                ProfessionNames = Get<Skill>().GetView(),
                HP = Get<Health>().HP,
                MHP = Get<Health>().MHP,
                DefenseView = Get<Defense>().GetView(),
                ResourcesView = Get<Resource>().GetView(),
                FutureAttackView = Get<TurnContext>().GetFutureAttackView(),
                FutureDefenseView = Get<TurnContext>().GetFutureDefenseView()
            };
        }
    }
    public class BodyView
    {
        public required string BodyName { get; set; }
        public required List<string> ProfessionNames { get; set; }
        public required int HP { get; set; }
        public required int MHP { get; set; }
        public required List<(string name, int power)> DefenseView { get; set; }
        public required List<(string name, float quantity)> ResourcesView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureAttackView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureDefenseView { get; set; }
    }
}
