using System.Formats.Tar;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Infra.Utils;
using BlacksmithCore.Specific.BuiltInProfessions;
namespace BlacksmithCore.Infra.DSL
{
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public static partial class BlacksmithDSL
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
            // Core
            #region 
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
                public Sentence(
                    Action<Community> structure,
                    SentenceType sentenceType,
                    StructureType structureType,
                    Sentence? bindSentence = null)
                {
                    Structure = structure;
                    SentenceType = sentenceType;
                    StructureType = structureType;
                    BindSentence = bindSentence;
                }
            }
            public bool IsPassive { get; set; } = false;

            private Dictionary<Type, SourceFile> _states = new();
            protected List<Sentence> _sentences = new();
            protected Stack<Sentence> _rhetoricCache = new();
            protected List<List<ICallbackOnJudge>> _callbacksOnCompile = new();
            private TFile SwitchState<TFile>()
                where TFile : SourceFile, new()
            {
                if(_states.TryGetValue(typeof(TFile), out var file))
                {
                    return (TFile)file;
                }
                TFile newFile = new()
                {
                    _states = _states,
                    _sentences = _sentences,
                    _rhetoricCache = _rhetoricCache,
                    _callbacksOnCompile = _callbacksOnCompile
                };
                _states[typeof(TFile)] = newFile;
                return newFile;
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
                        sentences.Insert(index, new(rhetoric.Structure, rhetoric.SentenceType, rhetoric.StructureType));
                    }
                }
                Action<Community> result = (a) => { };
                foreach (var sentence in sentences)
                {
                    result += sentence.Structure;
                }
                if (judgeRuleManager != null)
                {
                    result += _ =>
                    {
                        foreach (var callback in _callbacksOnCompile)
                        {
                            judgeRuleManager.AddJudgeRule(callback);
                        }
                    };
                }
                return new Intent() { Execute = result };
            }

            #endregion
            // Public API
            #region
            private static bool DefaultIfUndo() => false;
            // Analyzable
            #region
            public AttackFile WriteAttack(
                int power,
                AttackType.CEValue attackType,
                int delayRounds = 0,
                float APFactor = 1f,
                string analyzerKey = nameof(StandardAnalyzers.DefaultAttack),
                Func<bool>? ifUndo = null
            )
            {
                ifUndo ??= DefaultIfUndo;
                _sentences.Add(new((source) =>
                {
                    if (ifUndo())
                    {
                        return;
                    }
                    var analyzableData = new AttackAnalyzableData
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        Type = attackType,
                        Power = power,
                        APFactor = APFactor
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Attack, StructureType.Main));
                return SwitchState<AttackFile>();
            }

            public DefenseFile WriteDefense(
                DefenseEntity defense,
                int delayRounds = 0,
                string analyzerKey = nameof(StandardAnalyzers.DefaultDefense),
                Func<bool> ? ifUndo = null)
            {
                ifUndo ??= DefaultIfUndo;
                _sentences.Add(new((source) =>
                {
                    if (ifUndo())
                    {
                        return;
                    }
                    var analyzableData = new DefenseAnalyzableData()
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        Defense = defense,
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Defense, StructureType.Main));
                return SwitchState<DefenseFile>();
            }
            public ResourceFile WriteResource(
                float power,
                ResourceType.CEValue type,
                int delayRounds = 0,
                string analyzerKey = nameof(StandardAnalyzers.DefaultResource), 
                Func<bool>? ifUndo = null)
            {
                ifUndo ??= DefaultIfUndo;
                _sentences.Add(new((source) =>
                {
                    if (ifUndo())
                    {
                        return;
                    }
                    var analyzableData = new ResourceAnalyzableData()
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        Power = power,
                        Type = type
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Resource, StructureType.Main));
                return SwitchState<ResourceFile>();
            }
            public EffectFile WriteEffect(
                EffectType.CEValue type,
                EffectTargetType.CEValue targetType,
                ClapRoundClock entityClock,
                string analyzerKey,
                int delayRounds = 0,
                float power = 0,
                Func<bool>? ifUndo = null)
            {
                ifUndo ??= DefaultIfUndo;
                _sentences.Add(new((source) =>
                {
                    if (ifUndo())
                    {
                        return;
                    }
                    var analyzableData = new EffectAnalyzableData()
                    {
                        AnalyzerKey = analyzerKey,
                        Clock = new(delayRounds: delayRounds),
                        EntityClock = entityClock,
                        Type = type,
                        TargetType = targetType,
                        Power = power
                    };
                    source.Focus.Get<TurnContext>().WriteAnalyzableData(analyzableData);
                }, SentenceType.Effect, StructureType.Main));
                return SwitchState<EffectFile>();
            }
            #endregion
            // Pure CompileTime
            #region
            public SourceFile WriteFree(
                Action<Community> action,
                Func<bool>? ifUndo = null)
            {
                ifUndo ??= DefaultIfUndo;
                _sentences.Add(new(source =>
                {
                    if (ifUndo())
                    {
                        return;
                    }
                    action(source);
                }, SentenceType.Free, StructureType.Main));
                return (SourceFile)this;
            }
            public SourceFile UseResource(
                float need, 
                ResourceType.CEValue type, 
                bool ifCommonOnly = false,
                Func<bool>? ifUndo = null)
            {
                return WriteFree(
                    source => source.Focus.Get<Resource>().Use(type, need, ifCommonOnly),
                    ifUndo);
            }
            public SourceFile UseResource(
                Func<float> need,
                ResourceType.CEValue type,
                bool ifCommonOnly = false,
                Func<bool>? ifUndo = null)
            {
                return WriteFree(
                    source => source.Focus.Get<Resource>().Use(type, need(), ifCommonOnly),
                    ifUndo);
            }
            public SourceFile LoseHP(int loss, Func<bool>? ifUndo = null)
            {
                return WriteFree(
                    source => source.Focus.Get<Health>().LoseHP(loss),
                    ifUndo);
            }
            public SourceFile LoseMHP(int loss, Func<bool>? ifUndo = null)
            {
                return WriteFree(
                    source => source.Focus.Get<Health>().LoseMHP(loss),
                    ifUndo);
            }
            public SourceFile GainHP(int power, Func<bool>? ifUndo = null)
            {
                ifUndo ??= DefaultIfUndo;
                _sentences.Add(new((source) =>
                {
                    if (ifUndo())
                    {
                        return;
                    }
                    source.Focus.Get<Health>().GainHP(power);
                }, SentenceType.Recovery, StructureType.Main));
                return SwitchState<SourceFile>();
            }
            public SourceFile AddMark(string markName)
            {
                var entity = new EffectEntity()
                {
                    AnalyzerKey = markName,
                    IsMark = true,
                    Type = EffectType.Instance.Default(),
                    Clock = new(isInfinite: true)
                };
                return WriteFree(source =>
                {
                    source.Focus.Get<Effect>().Add(entity);
                });
            }
            public SourceFile Query<T>(Func<Community, T> path, out Lazy<T> lazyResult)
                where T : struct
            {
                bool isExecuted = false;
                T result = default;

                lazyResult = new Lazy<T>(() =>
                {
                    if (!isExecuted)
                        throw new InvalidOperationException(
                            $"尚未查询类型<{nameof(T)}>的数据。\n" +
                            $"请确保在在lambda阶段使用此数据");
                    return result;
                });

                return WriteFree(source =>
                {
                    result = path(source);
                    isExecuted = true;
                });
            }
            public SourceFile TakeMark(string markName, out Lazy<int> layerNum)
            {
                bool isExecuted = false;
                int result = 0;

                layerNum = new Lazy<int>(() =>
                {
                    if (!isExecuted)
                        throw new InvalidOperationException(
                            $"标记 '{markName}' 的计数尚未计算。\n" +
                            $"请确保在在lambda阶段使用此数据");
                    return result;
                });

                return WriteFree(source =>
                {
                    var effects = source.Focus.Get<Effect>().Effects;
                    var marks = effects.FindAll(m => m.AnalyzerKey == markName);
                    result = marks.Count;
                    isExecuted = true;
                    foreach (var mark in marks)
                    {
                        effects.Remove(mark);
                    }
                });
            }
            public SourceFile TakeMark(IReadOnlySet<string> markNames, out Lazy<IReadOnlyDictionary<string, int>> layerNums)
            {
                bool isExecuted = false;
                Dictionary<string, int> result = new();

                layerNums = new Lazy<IReadOnlyDictionary<string, int>>(() =>
                {
                    if (!isExecuted)
                        throw new InvalidOperationException(
                            $"标记组 '{markNames}' 的计数尚未计算。\n" +
                            $"请确保在在lambda阶段使用此数据");
                    return result;
                });

                return WriteFree(source =>
                {
                    var effects = source.Focus.Get<Effect>().Effects;
                    foreach (var markName in markNames)
                    {
                        var marks = effects.FindAll(m => m.AnalyzerKey == markName);
                        result[markName] = marks.Count;
                        isExecuted = true;
                        foreach (var mark in marks)
                        {
                            effects.Remove(mark);
                        }
                    }
                });
            }
            public SourceFile CountMark(string markName, out Lazy<int> layerNum)
            {
                bool isExecuted = false;
                int result = 0;

                layerNum = new Lazy<int>(() =>
                {
                    if (!isExecuted)
                        throw new InvalidOperationException(
                            $"标记 '{markName}' 的计数尚未计算。\n" +
                            $"请确保在在lambda阶段使用此数据");
                    return result;
                });

                return WriteFree(source =>
                {
                    var effects = source.Focus.Get<Effect>().Effects;
                    var marks = effects.FindAll(m => m.AnalyzerKey == markName);
                    result = marks.Count;
                    isExecuted = true;
                });
            }
            public SourceFile RegistCallbackOnJudge(
                List<ICallbackOnJudge> callbacks)
            {
                return WriteFree(source =>
                {
                    var isPlayer = source.IsPlayer;
                    foreach (var callback in callbacks)
                    {
                        callback.IsPlayer = isPlayer;
                    }
                    _callbacksOnCompile.Add(callbacks);
                });
            }
            public SourceFile AddMainProfession<TMainProfession>(bool isExclusive = true) 
                where TMainProfession : MainProfession, new()
            {
                return WriteFree(source =>
                {
                    var skill = source.Focus.Get<Skill>();
                    skill.AddPackage(new(new TMainProfession()));
                    if (isExclusive)
                    {
                        foreach (var name in ProfessionRegistry.MainProfessionSkillNames)
                        {
                            skill.RemoveSkill(nameof(Common), name);
                        }
                    }
                    else
                    {
                        skill.RemoveSkill(nameof(Common), nameof(TMainProfession).ToLower());
                    }
                });
            }
            public SourceFile AddMainProfession<TMainProfession>(IReadOnlySet<string> exclusionSet)
                where TMainProfession : MainProfession, new()
            {
                return WriteFree(source =>
                {
                    var skill = source.Focus.Get<Skill>();
                    skill.AddPackage(new(new TMainProfession()));
                    foreach (var name in exclusionSet)
                    {
                        skill.RemoveSkill(nameof(Common), name);
                    }
                    foreach (var name in ProfessionRegistry.MainProfessionSkillNames)
                    {
                        skill.RemoveSkill(nameof(Common), name);
                    }
                });
            }
            #endregion
            #endregion
            // Protected API
            #region
            protected TFile WithModify<TFile, TAnalyzableData>(Action<TAnalyzableData> modifier, SentenceType sentenceType)
                where TFile : SourceFile, new()
                where TAnalyzableData : IAnalyzableData
            {
                _rhetoricCache.Push(new((source) =>
                {
                    var list = source.Focus.Get<TurnContext>().Get<TAnalyzableData>();
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    modifier(last);
                }, sentenceType, StructureType.Rhetoric, _sentences[^1]));
                return SwitchState<TFile>();
            } 
            #endregion
        }
        public class DefenseFile : SourceFile
        {
            public DefenseFile WithModify(Action<DefenseAnalyzableData> modifier)
                => WithModify<DefenseFile, DefenseAnalyzableData>(modifier, SentenceType.Defense);
        }
        public class AttackFile : SourceFile
        {
            public AttackFile WithModify(Action<AttackAnalyzableData> modifier)
                => WithModify<AttackFile, AttackAnalyzableData>(modifier, SentenceType.Attack);
            public AttackFile WithCallback(
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
                     WithModify(last => last.ExtraParams[nameof(StandardAnalyzers.DefaultBloodSuck)] = percent)
                    .WithCallback(
                    AttackStage.Instance.OnEnd(),
                    nameof(StandardAnalyzers.DefaultBloodSuck));
            }
            public AttackFile WithInterupt()
            {

                return WithCallback(
                    AttackStage.Instance.OnHitArmorFirstTime(),
                    nameof(StandardAnalyzers.DefaultInterupt));
            }
        }
        public class ResourceFile : SourceFile
        {
            public ResourceFile WithModify(Action<ResourceAnalyzableData> modifier)
                => WithModify<ResourceFile, ResourceAnalyzableData>(modifier, SentenceType.Resource);
        }
        public class EffectFile : SourceFile
        {
            public EffectFile WithModify(Action<EffectAnalyzableData> modifier)
                => WithModify<EffectFile, EffectAnalyzableData>(modifier, SentenceType.Effect);
        }

        public static SourceFile CreateBy(Pen Pen)
        {
            return Pen(new());
        }
    }
}
