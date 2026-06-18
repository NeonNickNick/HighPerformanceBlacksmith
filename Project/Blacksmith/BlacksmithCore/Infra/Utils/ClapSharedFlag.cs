namespace BlacksmithCore.Infra.Utils
{
    public class ClapSharedFlag
    {
        private int _times = 0;
        public bool IsActive => _times <= 0;
        public ClapSharedFlag Copy()
        {
            var copy = new ClapSharedFlag();
            copy._times = _times;
            return copy;
        }
        public void Disable()
        {
            _times++;
        }
        public void Enable()
        {
            if (_times <= 0)
            {
                return;
            }
            _times--;
        }
    }
}
