namespace BlacksmithCore.Infra.Attributes.Analyzer
{
    public enum AnalyzerType
    {
        DSL,
        Defense,
        JudgeCallback,
        Universal
    }
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsAnalyzer : Attribute
    {
        public readonly AnalyzerType Type;
        public IsAnalyzer(AnalyzerType type = AnalyzerType.DSL)
        {
            Type = type;
        }
    }
}
