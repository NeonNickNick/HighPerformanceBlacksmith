namespace BlacksmithCore.Infra.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Class,
        AllowMultiple = false, Inherited = false)]
    public class IsManual : Attribute
    {
    }
}