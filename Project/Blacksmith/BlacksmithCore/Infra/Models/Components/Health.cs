using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Entities;

namespace BlacksmithCore.Infra.Models.Components
{
    public class Health : IComponent<Body>
    {
        private int _hp = 10;
        private bool _killed = false;
        public bool IsKilled => _killed;
        public int HP
        {
            get => _hp;
            set
            {
                if (value <= 0)
                {
                    _killed = true;
                }
                _hp = value;
            }
        }
        public int MHP { get; set; } = 10;
        public void Copy(Health origin)
        {
            _hp = origin._hp;
            MHP = origin.MHP;
            _killed = origin._killed;
        }
        public void GainHP(int addition)
        {
            if (_killed)
            {
                return;
            }
            HP = (int)MathF.Min(MHP, HP + addition);
        }
        public void GainMHP(int addition)
        {
            if (_killed)
            {
                return;
            }
            MHP += addition;
        }
        public void LoseHP(int loss)
        {
            HP = HP - loss;
        }
        public void LoseMHP(int loss)
        {
            MHP = Math.Max(0, MHP - loss);
            HP = Math.Min(MHP, HP);
        }
    }
}