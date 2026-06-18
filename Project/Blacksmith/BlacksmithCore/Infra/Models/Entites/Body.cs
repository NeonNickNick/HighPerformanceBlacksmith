using BlacksmithCore.Infra.Models.Components;
using ClapInfra.ClapModels.Entities;

namespace BlacksmithCore.Infra.Models.Entites
{
    public class Body : ClapBody<Body>
    {
        private string _name;
        public Community Community { get; }
        public Body(Community community, string name)
        {
            _name = name;
            Community = community;
            Add(new()
        {
            new Skill(),
            new Health(),
            new Defense(),
            new Resource(),
            new Effect(),
            new TurnContext()
        });
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