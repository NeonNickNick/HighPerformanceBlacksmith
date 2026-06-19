using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClapSourceGenerators.Analyzer
{
    [Generator]
    public class AnalyzableDataCopyGenerator : IIncrementalGenerator
    {
        #region Diagnostic Descriptors

        /// <summary>CLAP010: Nested class implementing IAnalyzableData.</summary>
        private static readonly DiagnosticDescriptor NestedClassRule = new(
            id: "CLAP010",
            title: "IAnalyzableData 实现类是嵌套类",
            messageFormat: "类 '{0}' 是嵌套类（定义在 '{1}' 内部），源生成器不对嵌套类生成 Copy 方法。请将类移除外层类或手动实现 Copy。",
            category: "ClapAnalyzableDataCopy",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>CLAP011: Class implementing IAnalyzableData is not partial.</summary>
        private static readonly DiagnosticDescriptor NotPartialRule = new(
            id: "CLAP011",
            title: "IAnalyzableData 实现类未声明 partial",
            messageFormat: "类 '{0}' 实现了 IAnalyzableData 但未声明为 partial。请添加 partial 关键字以允许源生成器生成 Copy 方法。",
            category: "ClapAnalyzableDataCopy",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>CLAP012: Class contains fields.</summary>
        private static readonly DiagnosticDescriptor FieldNotAllowedRule = new(
            id: "CLAP012",
            title: "IAnalyzableData 实现类包含字段",
            messageFormat: "类 '{0}' 包含字段 '{1}'。IAnalyzableData 实现类只允许属性和方法，不允许字段。",
            category: "ClapAnalyzableDataCopy",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>CLAP013: Class has get-only property.</summary>
        private static readonly DiagnosticDescriptor GetOnlyPropertyRule = new(
            id: "CLAP013",
            title: "IAnalyzableData 实现类存在仅有 get 的属性",
            messageFormat: "类 '{0}' 的属性 '{1}' 仅有 get 访问器（无 set 或 init），无法在 Copy 方法中赋值。",
            category: "ClapAnalyzableDataCopy",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>CLAP014: Class has unsupported reference type members.</summary>
        private static readonly DiagnosticDescriptor ComplexRefTypeRule = new(
            id: "CLAP014",
            title: "IAnalyzableData 实现类包含无法自动拷贝的引用类型成员",
            messageFormat: "类 '{0}' 的属性 '{1}' 是引用类型 '{2}'，无法自动深拷贝。请添加 [IsManual] 特性或手动实现 Copy 方法。",
            category: "ClapAnalyzableDataCopy",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        #endregion

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetClassInfo(ctx))
                .Where(info => info is not null)
                .Collect();

            context.RegisterSourceOutput(provider, GenerateSource);
        }

        private static ClassInfo? GetClassInfo(GeneratorSyntaxContext ctx)
        {
            var classDecl = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol is not { IsAbstract: false })
                return null;

            var compilation = ctx.SemanticModel.Compilation;

            // Check if this class implements IAnalyzableData
            if (!ImplementsIAnalyzableData(symbol, compilation))
                return null;

            var filePath = classDecl.SyntaxTree.FilePath;
            var location = classDecl.Identifier.GetLocation();
            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            var className = symbol.Name;

            // Check for [IsManual] attribute — skip entirely if present
            if (HasIsManualAttribute(symbol, compilation))
                return new ClassInfo(className, namespaceName, filePath, location,
                    isManual: true, isNested: false, isPartial: false,
                    diagnostics: ImmutableArray<DiagnosticInfo>.Empty,
                    copyProperties: ImmutableArray<CopyPropertyInfo>.Empty);

            var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();
            bool isNested = symbol.ContainingType != null;
            bool isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

            // CLAP003: nested class
            if (isNested)
            {
                diagnostics.Add(new DiagnosticInfo(
                    DiagnosticKind.NestedClass,
                    location,
                    className,
                    symbol.ContainingType!.Name));
            }

            // CLAP004: not partial
            if (!isPartial)
            {
                diagnostics.Add(new DiagnosticInfo(
                    DiagnosticKind.NotPartial,
                    location,
                    className));
            }

            // Analyze members: collect copyable properties and warnings
            var copyProperties = ImmutableArray.CreateBuilder<CopyPropertyInfo>();
            AnalyzeMembers(symbol, compilation, diagnostics, copyProperties, location, className);

            return new ClassInfo(className, namespaceName, filePath, location,
                isManual: false, isNested: isNested, isPartial: isPartial,
                diagnostics: diagnostics.ToImmutable(),
                copyProperties: copyProperties.ToImmutable());
        }

        /// <summary>
        /// Analyze all directly-declared members of the class.
        /// - Fields → CLAP005
        /// - Get-only properties (no set/init) → CLAP006
        /// - Reference type properties (not string, not ClapRoundClock, not delegate) → CLAP007
        /// - Safe properties → collect for Copy() generation
        /// </summary>
        private static void AnalyzeMembers(
            INamedTypeSymbol symbol,
            Compilation compilation,
            ImmutableArray<DiagnosticInfo>.Builder diagnostics,
            ImmutableArray<CopyPropertyInfo>.Builder copyProperties,
            Location classLocation,
            string className)
        {
            var clockType = compilation.GetTypeByMetadataName("BlacksmithCore.Infra.Utils.ClapRoundClock");

            foreach (var member in symbol.GetMembers())
            {
                // Only consider members directly declared in this class (not inherited)
                if (!SymbolEqualityComparer.Default.Equals(member.ContainingType, symbol))
                    continue;

                // Skip static members
                if (member.IsStatic)
                    continue;

                // Skip methods (they are allowed)
                if (member is IMethodSymbol)
                    continue;

                // CLAP012: Fields are not allowed (skip compiler-generated backing fields)
                if (member is IFieldSymbol field)
                {
                    // Skip compiler-generated fields (auto-property backing fields, etc.)
                    if (field.IsImplicitlyDeclared)
                        continue;

                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticKind.FieldNotAllowed,
                        member.Locations.FirstOrDefault() ?? classLocation,
                        className,
                        field.Name));
                    continue;
                }

                // Process properties
                if (member is IPropertySymbol prop)
                {
                    // Skip indexers (this[])
                    if (prop.IsIndexer)
                        continue;

                    // CLAP006: Get-only property (no setter/init)
                    if (prop.SetMethod == null)
                    {
                        diagnostics.Add(new DiagnosticInfo(
                            DiagnosticKind.GetOnlyProperty,
                            member.Locations.FirstOrDefault() ?? classLocation,
                            className,
                            prop.Name));
                        continue;
                    }

                    // Check the property type
                    var propType = prop.Type;
                    var typeKind = ClassifyType(propType, clockType);

                    switch (typeKind)
                    {
                        case TypeClassification.SafeValueType:
                        case TypeClassification.SafeString:
                        case TypeClassification.SafeClock:
                            // Safe for Copy generation
                            copyProperties.Add(new CopyPropertyInfo(
                                prop.Name,
                                needsClockCopy: typeKind == TypeClassification.SafeClock));
                            break;

                        case TypeClassification.UnsafeRefType:
                            diagnostics.Add(new DiagnosticInfo(
                                DiagnosticKind.ComplexRefType,
                                member.Locations.FirstOrDefault() ?? classLocation,
                                className,
                                prop.Name,
                                propType.ToDisplayString()));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Classify a property type for Copy safety.
        /// </summary>
        private static TypeClassification ClassifyType(ITypeSymbol type, INamedTypeSymbol? clockType)
        {
            // Value types are safe
            if (type.IsValueType)
                return TypeClassification.SafeValueType;

            // string is immutable, safe
            if (type.SpecialType == SpecialType.System_String)
                return TypeClassification.SafeString;

            // ClapRoundClock has Copy() method
            if (clockType != null && SymbolEqualityComparer.Default.Equals(type, clockType))
                return TypeClassification.SafeClock;

            // Everything else (including delegates) is unsafe
            return TypeClassification.UnsafeRefType;
        }

        /// <summary>
        /// Check if the class implements IAnalyzableData (directly or via base types/interfaces).
        /// </summary>
        private static bool ImplementsIAnalyzableData(INamedTypeSymbol symbol, Compilation compilation)
        {
            var targetInterface = compilation.GetTypeByMetadataName(
                "BlacksmithCore.Infra.Models.Components.IAnalyzableData");
            if (targetInterface == null)
                return false;

            return symbol.AllInterfaces.Any(
                i => SymbolEqualityComparer.Default.Equals(i, targetInterface));
        }

        /// <summary>
        /// Check if the class has the [IsManual] attribute.
        /// </summary>
        private static bool HasIsManualAttribute(INamedTypeSymbol symbol, Compilation compilation)
        {
            var isManualType = compilation.GetTypeByMetadataName(
                "BlacksmithCore.Infra.Attributes.Analyzer.IsManual");
            if (isManualType == null)
                return false;

            return symbol.GetAttributes().Any(
                attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, isManualType));
        }

        private static void GenerateSource(SourceProductionContext ctx, ImmutableArray<ClassInfo?> infos)
        {
            // Deduplicate by (Namespace, ClassName) — partial classes produce
            // one entry per file, but we want one output per class.
            var seen = new HashSet<string>();
            foreach (var info in infos)
            {
                if (info == null)
                    continue;

                var classKey = $"{info.Namespace}.{info.ClassName}";
                if (!seen.Add(classKey))
                    continue;

                // Skip classes marked [IsManual]
                if (info.IsManual)
                    continue;

                // Report all diagnostics
                foreach (var diag in info.Diagnostics)
                {
                    var descriptor = diag.Kind switch
                    {
                        DiagnosticKind.NestedClass => NestedClassRule,
                        DiagnosticKind.NotPartial => NotPartialRule,
                        DiagnosticKind.FieldNotAllowed => FieldNotAllowedRule,
                        DiagnosticKind.GetOnlyProperty => GetOnlyPropertyRule,
                        DiagnosticKind.ComplexRefType => ComplexRefTypeRule,
                        _ => NotPartialRule // fallback
                    };

                    var location = diag.Location
                        ?? Location.Create(info.FilePath,
                            new Microsoft.CodeAnalysis.Text.TextSpan(0, 0),
                            new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                                new Microsoft.CodeAnalysis.Text.LinePosition(0, 0),
                                new Microsoft.CodeAnalysis.Text.LinePosition(0, 0)));

                    ctx.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        location,
                        diag.Args));
                }

                // Only generate Copy() if zero diagnostics
                if (info.Diagnostics.Length == 0)
                {
                    ctx.AddSource(
                        $"{info.Namespace}.{info.ClassName}.Copy.g.cs",
                        GeneratePartialClass(info));
                }
            }
        }

        private static string GeneratePartialClass(ClassInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine($"namespace {info.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"partial class {info.ClassName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public {info.ClassName} Copy()");
            sb.AppendLine("    {");
            sb.AppendLine($"        return new {info.ClassName}");
            sb.AppendLine("        {");

            for (int i = 0; i < info.CopyProperties.Length; i++)
            {
                var prop = info.CopyProperties[i];
                var separator = i < info.CopyProperties.Length - 1 ? "," : "";
                if (prop.NeedsClockCopy)
                {
                    sb.AppendLine($"            {prop.Name} = this.{prop.Name}.Copy(){separator}");
                }
                else
                {
                    sb.AppendLine($"            {prop.Name} = this.{prop.Name}{separator}");
                }
            }

            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #region Internal Types

        private enum DiagnosticKind
        {
            NestedClass,
            NotPartial,
            FieldNotAllowed,
            GetOnlyProperty,
            ComplexRefType
        }

        private enum TypeClassification
        {
            SafeValueType,
            SafeString,
            SafeClock,
            UnsafeRefType
        }

        private sealed class DiagnosticInfo
        {
            public DiagnosticKind Kind { get; }
            public Location? Location { get; }
            public string[] Args { get; }

            public DiagnosticInfo(DiagnosticKind kind, Location? location, params string[] args)
            {
                Kind = kind;
                Location = location;
                Args = args;
            }
        }

        private sealed class CopyPropertyInfo
        {
            public string Name { get; }
            public bool NeedsClockCopy { get; }

            public CopyPropertyInfo(string name, bool needsClockCopy)
            {
                Name = name;
                NeedsClockCopy = needsClockCopy;
            }
        }

        private sealed class ClassInfo
        {
            public string ClassName { get; }
            public string Namespace { get; }
            public string FilePath { get; }
            public Location Location { get; }
            public bool IsManual { get; }
            public bool IsNested { get; }
            public bool IsPartial { get; }
            public ImmutableArray<DiagnosticInfo> Diagnostics { get; }
            public ImmutableArray<CopyPropertyInfo> CopyProperties { get; }

            public ClassInfo(
                string className,
                string @namespace,
                string filePath,
                Location location,
                bool isManual,
                bool isNested,
                bool isPartial,
                ImmutableArray<DiagnosticInfo> diagnostics,
                ImmutableArray<CopyPropertyInfo> copyProperties)
            {
                ClassName = className;
                Namespace = @namespace;
                FilePath = filePath;
                Location = location;
                IsManual = isManual;
                IsNested = isNested;
                IsPartial = isPartial;
                Diagnostics = diagnostics;
                CopyProperties = copyProperties;
            }
        }

        #endregion
    }
}
