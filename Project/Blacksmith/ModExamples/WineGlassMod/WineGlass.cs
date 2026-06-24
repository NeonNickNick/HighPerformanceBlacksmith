using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;

namespace ModExamples.WineGlassMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class WineGlass : MainProfession
    {
        private bool WineCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private IDSLSourceFile Wine(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1f, ResourceType.Instance.Wine());
            return DSL.CreateBy(pen);
        }
        private bool CarnivalCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wine(), 3f);
        }
        private IDSLSourceFile Carnival(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Wine())
                .WriteEffect(
                    EffectType.Instance.AfterTransport(),
                    EffectTargetType.Instance.Self(),
                    power: 0f,
                    duration: 1,
                    (source, target, entity) =>
                    {
                        var alist = target.Get<TurnContext>().Get<AttackAnalyzableData>();
                        target.Get<TurnContext>().Get<AttackAnalyzableData>()
                              .RemoveAll(a => a.Clock.IsRinging);
                    }
                );
            return DSL.CreateBy(pen);
        }
        private bool ForgetCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Wine(), 3f);
        }
        private IDSLSourceFile Forget(ISkillCheckContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Wine())
                .WriteEffect(
                    EffectType.Instance.AfterAnalyzableDataWritten(),
                    EffectTargetType.Instance.Self(),
                    power: 0f,
                    duration: 3,
                    (source, target, entity) =>
                    {
                        var alist = target.Get<TurnContext>().Get<AttackAnalyzableData>();
                        target.Get<TurnContext>().Get<AttackAnalyzableData>()
                              .RemoveAll(a => a.Clock.IsRinging);
                    },
                    delayRounds: 1
                );
            return DSL.CreateBy(pen);
        }
    }
}
