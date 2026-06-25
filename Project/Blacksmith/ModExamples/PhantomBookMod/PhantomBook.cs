using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Attributes.SkillMarkOnly;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using ModExamples.PhantomBookMod.Defense;

namespace ModExamples.PhantomBookMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsExperimental]
    public partial class PhantomBook : MainProfession
    {
        private bool FantasiaCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private IDSLSourceFile Fantasia(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Dream());
            return DSL.CreateBy(pen);
        }

        private bool AssociationCheck(ISkillCheckContext sc)
        {
            string expectedSkill = sc.SkillDeclareData.StringParam;
            var swapInstance = sc.SudoOperations.DeepCopy();
            var fakeSelf = sc.SudoOperations.IsPlayer(sc.Self) ? swapInstance.Enemy : swapInstance.Player;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext
            {
                SudoOperations = swapInstance,
                Self = fakeSelf,
                SkillDeclareData = SkillDeclareData.Parse($"{expectedSkill}(p:{sc.SkillDeclareData.Param},s:{sc.SkillDeclareData.StringParam})")!
            };
            if (sc.SudoOperations.GameMetadata.EquipmentSkillNames.Contains(expectedSkill) ||
                sc.SudoOperations.GameMetadata.MainProfessionSkillNames.Contains(expectedSkill) ||
                expectedSkill == $"{nameof(Association).ToLower()}" ||
                fakeSkill.TryDeclare(fsc.SkillDeclareData.SkillName, fsc) != SkillDeclareResult.Success)
            {
                return false;
            }
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        private IDSLSourceFile Association(ISkillExecuteContext sc)
        {
            string expectedSkill = sc.SkillDeclareData.StringParam;
            var swapInstance = sc.SudoOperations.DeepCopy();
            var fakeSelf = sc.SudoOperations.IsPlayer(sc.Self) ? swapInstance.Enemy : swapInstance.Player;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext
            {
                SudoOperations = swapInstance,
                Self = fakeSelf,
                SkillDeclareData = SkillDeclareData.Parse($"{expectedSkill}(p:{sc.SkillDeclareData.Param},s:{sc.SkillDeclareData.StringParam})")!
            };
            var stolenSF = fakeSkill.Declare(fsc.SkillDeclareData.SkillName, fsc);
            stolenSF.Move(sc.Self);
            return ((DSL.SourceFile)stolenSF)
                .UseResource(2f, ResourceType.Instance.Dream());
        }
        private bool HallucinateCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        private IDSLSourceFile Hallucinate(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
               .UseResource(2f, ResourceType.Instance.Dream())
               .WriteEffect(EffectType.Instance.AfterAnalyzableDataWritten(), EffectTargetType.Instance.Enemy(), 0, 1,
               (Community source, Body main, EffectEntity effectEntity) =>
               {
                   var tc = main.Get<TurnContext>();
                   tc.Get<AttackAnalyzableData>().ForEach(a => a.Clock.DelayRounds++);
                   tc.AddPreprocess<AttackAnalyzableData>(a => a.Clock.DelayRounds++, isInfinite: true);
               });
            return DSL.CreateBy(pen);
        }
        private bool AwakeningCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        [IsHighCost]
        private IDSLSourceFile Awakening(ISkillExecuteContext sc)
        {
            var sandBoxInstance = sc.SudoOperations.DeepCopy(preRounds: 3);
            Body copiedBody = sc.SudoOperations.IsPlayer(sc.Self) ? sandBoxInstance.Player.Focus : sandBoxInstance.Enemy.Focus;
            var resource = copiedBody.Get<Resource>();
            float m = MathF.Min(2f, resource.QueryAll(ResourceType.Instance.Dream()));
            resource.Use(ResourceType.Instance.Dream(), m);
            sc.Self.AddTransform(() =>
            {
                sc.Self.Focus = copiedBody;
            });
            return DSL.Create(sc.Self, _ => _);
        }
        private bool IllusionCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 1f);
        }
        private IDSLSourceFile Illusion(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .GainHP(5);
            return DSL.CreateBy(pen);
        }
        private bool NightmareCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 3f)
                && sc.Self.Focus.Get<Health>().HP > 1;
        }
        [IsExperimental]
        [IsEquipmentSkill]
        private IDSLSourceFile Nightmare(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .LoseHP(5)
                .WriteDefense(0f, new PhysicalImmunity())
                .WriteDefense(6f, new NightmareArmor(owner =>
                {
                    owner.Focus.Get<Skill>().AddSkill(nameof(PhantomBook), nameof(Nightmare));
                    owner.Focus.Get<Skill>().RemovePackage(nameof(Nightmare));
                }))
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(PhantomBook), nameof(Nightmare));
                    source.Focus.Get<Skill>().AddPackage(new(new Nightmare()));
                }, false);
            return DSL.CreateBy(pen);
        }
    }
}
