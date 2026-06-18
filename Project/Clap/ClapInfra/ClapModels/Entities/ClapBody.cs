namespace ClapInfra.ClapModels.Entities
{
    public interface IUpdatePerRound
    {
        public void Update();
    }
    public interface IComponent<TBody>
    {

    }
    public abstract class ClapBody<TBody>
        where TBody : ClapBody<TBody>
    {
        protected Dictionary<Type, IComponent<TBody>> _components = new();
        protected void Add(HashSet<IComponent<TBody>> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }
            foreach (var obj in components)
            {
                if (obj == null)
                {
                    throw new ArgumentException("ClapBody component cannot be null!");
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
            where TTargetComponent : IComponent<TBody>
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
    }
}
