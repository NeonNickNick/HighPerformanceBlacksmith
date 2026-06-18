using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.DSL
{
    public class AnalyzerRegistry<TDelegate>
        where TDelegate : Delegate
    {
        private static readonly Dictionary<string, TDelegate> _analyzerDict = new();
        public void Regist(string key, TDelegate @delegate)
        {
            _analyzerDict[key] = @delegate;
        }
        public TDelegate Get(string key)
        {
            return _analyzerDict[key];
        }
    }
    public class UniversalRegistry
    {
        private static readonly Dictionary<Type, Dictionary<string, Delegate>> _analyzerDict = new();
        public void Regist<TDelegate>(string key, TDelegate @delegate)
            where TDelegate : Delegate
        {
            if (!_analyzerDict.TryGetValue(typeof(TDelegate), out var _))
            {
                _analyzerDict[typeof(TDelegate)] = new();
            }
            _analyzerDict[typeof(TDelegate)][key] = @delegate;
        }
        public TDelegate Get<TDelegate>(string key)
            where TDelegate : Delegate
        {
            return (TDelegate)_analyzerDict[typeof(TDelegate)][key];
        }
    }
    public static class AnalyzerRegistry
    {
        public static readonly AnalyzerRegistry<Action<Community, Community, IAnalyzableData>> DSL = new();
        public static readonly AnalyzerRegistry<Action<Community, Community, DefenseEntity, AttackAnalyzableData>> Defense = new();
        public static readonly AnalyzerRegistry<Action<Community, Community>> JudgeCallback = new();
        public static readonly UniversalRegistry Universal = new();
    }
}
