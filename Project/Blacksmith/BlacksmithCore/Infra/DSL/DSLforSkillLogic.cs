using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Utils;
namespace BlacksmithCore.Infra.DSL
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public static class DSLforSkillLogic
    {
        public enum SentenceType
        {
            Attack,
            Defense,
            Resource,
            Effect,
            Recovery,
            Free
        }

        public class SourceFile : IDSLSourceFile
        {
            protected enum StructureType
            {
                Main,
                Rhetoric
            }
            protected class Sentence
            {
                public Action<Community> Structure { get; }
                public SentenceType SentenceType { get; }
                public StructureType StructureType { get; }
                public Sentence? BindSentence { get; }
                public Sentence(Action<Community> structure, SentenceType sentenceType, StructureType structureType, Sentence? bindSentence = null)
                {
                    Structure = structure;
                    SentenceType = sentenceType;
                    StructureType = structureType;
                    BindSentence = bindSentence;
                }
            }
            public bool IsPassive { get; set; } = false;

            protected Community _owner;
            protected List<Sentence> _sentences = new();
            protected Stack<Sentence> _rhetoricCache = new();
            protected List<List<ICallbackOnJudge>> _mutationsOnCompile = new();
            protected SourceFile(SourceFile origin)
            {
                _owner = origin._owner;
                _sentences = origin._sentences;
                _rhetoricCache = origin._rhetoricCache;
                _mutationsOnCompile = origin._mutationsOnCompile;
            }
            public SourceFile(Community owner)
            {
                _owner = owner;
            }
            public void Move(Community newOwner, HashSet<SentenceType> filter)
            {
                _owner = newOwner;
                _sentences.RemoveAll(s => filter.Contains(s.SentenceType));
            }
            public Intent Compile(JudgeRuleManager? judgeRuleManager = null)
            {
                List<Sentence> sentences = new(_sentences);
                int n = _rhetoricCache.Count;
                for (int i = 0; i < n; ++i)
                {
                    var rhetoric = _rhetoricCache.Pop();
                    int index = sentences.IndexOf(rhetoric.BindSentence!) + 1;
                    if (index > 0)
                    {
                        sentences.Insert(index, new(rhetoric.Structure, rhetoric.SentenceType, StructureType.Rhetoric));
                    }
                }
                Action<Community> result = (a) => { };
                if (judgeRuleManager != null)
                {
                    foreach (var temp in _mutationsOnCompile)
                    {
                        judgeRuleManager.AddJudgeRule(_owner, temp);
                    }
                }
                foreach (var sentence in sentences)
                {
                    result += sentence.Structure;
                }
                return new Intent() { Execute = result };
            }
            public SourceFile WriteCompileTime(Action<Community> action)
            {
                _sentences.Add(new(action, SentenceType.Free, StructureType.Main));
                return this;
            }
            public AttackFile WriteAttack(
                int power,
                AttackType.CEValue attackType,
                int delayRounds = 0,
                float aPFactor = 1f,
                string analyzerKey = nameof(StandardAnalyzers.DefaultAttack)
            )
            {
                _sentences.Add(new((source) =>
                {
                    var analyzableData = new AttackAnalyzableData
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        Type = attackType,
                        Power = power,
                        APFactor = aPFactor
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Attack, StructureType.Main));
                return new(this);
            }

            public RecoveryFile WriteRecovery(int power)
            {
                _sentences.Add(new((source) =>
                {
                    source.Focus.Get<Health>().GainHP(power);
                }, SentenceType.Recovery, StructureType.Main));
                return new(this);
            }
            public DefenseFile WriteDefense(
                DefenseEntity defense,
                int delayRounds = 0,
                string analyzerKey = nameof(StandardAnalyzers.DefaultDefense)
            )
            {
                _sentences.Add(new((source) =>
                {
                    var analyzableData = new DefenseAnalyzableData()
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        Defense = defense,
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Defense, StructureType.Main));
                return new(this);
            }
            public ResourceFile WriteResource(
                float power,
                ResourceType.CEValue type,
                int delayRounds = 0,
                string analyzerKey = nameof(StandardAnalyzers.DefaultResource)
            )
            {
                _sentences.Add(new((source) =>
                {
                    var analyzableData = new ResourceAnalyzableData()
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        Power = power,
                        Type = type
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Resource, StructureType.Main));
                return new(this);
            }
            public EffectFile WriteEffect(
                EffectType.CEValue type,
                EffectTargetType.CEValue targetType,
                ClapRoundClock entityClock,
                string analyzerKey,
                int delayRounds = 0,
                float power = 0
                )
            {
                _sentences.Add(new((source) =>
                {
                    var analyzableData = new EffectAnalyzableData()
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        EntityClock = entityClock,
                        Type = type,
                        TargetType = targetType,
                        Power = power
                    };/*
                    analyzableData.Execute = (target) =>
                    {
                        Body main = target.Focus;
                        var entity = new EffectEntity(analyzableData.Type, analyzableData.Power, new(remainingRounds: duration));
                        entity.Execute = (body) => effectAction(source, body, entity);
                        main.Get<Effect>().Add(entity);
                    };*/
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Effect, StructureType.Main));
                return new(this);
            }
            public SourceFile UseResource(float need, ResourceType.CEValue type, bool ifCommonOnly = false)
            {
                return WriteCompileTime(source => source.Focus.Get<Resource>().Use(type, need, ifCommonOnly));
            }
            public SourceFile LoseHP(int loss)
            {
                return WriteCompileTime(source => source.Focus.Get<Health>().LoseHP(loss));
            }
            public SourceFile LoseMHP(int loss)
            {
                return WriteCompileTime(source => source.Focus.Get<Health>().LoseMHP(loss));
            }
            public SourceFile AddMark(EffectEntity entity)
            {
                entity.IsMark = true;
                return WriteCompileTime(source =>
                {
                    source.Focus.Get<Effect>().Add(entity);
                });
            }
            public SourceFile RegistCallbackOnJudge(
                List<ICallbackOnJudge> mutations)
            {
                _mutationsOnCompile.Add(mutations);
                return this;
            }

        }
        public class DefenseFile : SourceFile
        {
            public DefenseFile(SourceFile self) : base(self)
            {
            }
        }
        public class RecoveryFile : SourceFile
        {
            public RecoveryFile(SourceFile self) : base(self)
            {
            }
        }
        public class AttackFile : SourceFile
        {
            public AttackFile WithComplieTime(Action<AttackAnalyzableData> modifier)
            {
                _rhetoricCache.Push(new((source) =>
                {
                    var list = source.Focus.Get<TurnContext>().Get<AttackAnalyzableData>();
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    modifier(last);
                }, SentenceType.Attack, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public AttackFile WithRuntime(
                AttackStage.CEValue stage,
                string analyzerKey
            )
            {
                _rhetoricCache.Push(new((source) =>
                {
                    var list = source.Focus.Get<TurnContext>().Get<AttackAnalyzableData>();
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    if (!last.StageKeys.TryGetValue(stage, out var _))
                    {
                        last.StageKeys[stage] = new();
                    }
                    last.StageKeys[stage].Add(analyzerKey);
                }, SentenceType.Attack, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public AttackFile WithBloodSuck(float percent)
            {
                return
                     WithComplieTime(last => last.ExtraParams[nameof(StandardAnalyzers.DefaultBloodSuck)] = percent)
                    .WithRuntime(
                    AttackStage.Instance.OnEnd(),
                    nameof(StandardAnalyzers.DefaultBloodSuck));
            }
            public AttackFile WithInterupt()
            {

                return WithRuntime(
                    AttackStage.Instance.OnHitArmorFirstTime(),
                    nameof(StandardAnalyzers.DefaultInterupt));
            }
            public AttackFile(SourceFile self) : base(self)
            {
            }
        }
        public class ResourceFile : SourceFile
        {
            public ResourceFile(SourceFile self) : base(self)
            {
            }
        }
        public class EffectFile : SourceFile
        {
            public EffectFile(SourceFile self) : base(self)
            {
            }
        }

        public static SourceFile Create(Community source, Pen Pen)
        {
            var sourceFile = new SourceFile(source);
            return Pen(sourceFile);
        }
    }
}
