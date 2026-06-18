using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Judgement.Core
{
    public class Intent
    {
        public required Action<Community> Execute { get; set; }
    }
}
