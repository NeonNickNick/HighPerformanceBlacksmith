namespace BlacksmithCore.Infra.Utils
{
    public class ClapRoundClock
    {
        private int _delayRounds = 0;
        private int _remainingRounds = 1;
        private bool _isInfinite = false;
        public int DelayRounds { get => _delayRounds; private set => _delayRounds = value; }
        public int RemainingRounds { get => _remainingRounds; private set => _remainingRounds = value; }
        public bool IsRinging => _delayRounds == 0 && _remainingRounds > 0;
        public bool IsDead => _remainingRounds <= 0;
        public ClapRoundClock()
        {
            _delayRounds = 0;
            _remainingRounds = 1;
            _isInfinite = false;
        }
        public ClapRoundClock(
            int delayRounds = 0,
            int remainingRounds = 1,
            bool isInfinite = false)
        {
            _delayRounds = delayRounds;
            _remainingRounds = remainingRounds;
            _isInfinite = isInfinite;
        }
        public void SetDelay(int delayRounds)
        {
            DelayRounds = delayRounds;
        }
        public void RoundPass()
        {
            if (_delayRounds > 0)
            {
                _delayRounds--;
                return;
            }
            if (!_isInfinite)
            {
                _remainingRounds--;
            }
        }
        public void Kill()
        {
            _delayRounds = 0;
            _remainingRounds = 0;
            _isInfinite = false;
        }

        public ClapRoundClock Copy()
        {
            return new(delayRounds: _delayRounds, remainingRounds: _remainingRounds, isInfinite: _isInfinite);
        }
    }
}
