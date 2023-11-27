using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.Tracing;

namespace OneOf.TypeExtensions.CodeFix
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OneOfTypeExtensionsCodeFixAnalyzer : DiagnosticAnalyzer
    {
        public const string IsTypeDiagnosticId = "OneOfTypeExtensionsIsType";
        public const string AsTypeDiagnosticId = "OneOfTypeExtensionsAsType";
        public const string MapTypeDiagnosticId = "OneOfTypeExtensionsMapType";
        public const string TryPickTypeDiagnosticId = "OneOfTypeExtensionsTryPickType";

        private static readonly LocalizableString IsTitle = new LocalizableResourceString(nameof(Resources.IsAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString IsMessageFormat = new LocalizableResourceString(nameof(Resources.IsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString IsDescription = new LocalizableResourceString(nameof(Resources.IsAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString AsTitle = new LocalizableResourceString(nameof(Resources.AsAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsMessageFormat = new LocalizableResourceString(nameof(Resources.AsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsDescription = new LocalizableResourceString(nameof(Resources.AsAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MapTitle = new LocalizableResourceString(nameof(Resources.MapAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MapMessageFormat = new LocalizableResourceString(nameof(Resources.MapAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MapDescription = new LocalizableResourceString(nameof(Resources.MapAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString TryPickTitle = new LocalizableResourceString(nameof(Resources.TryPickAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TryPickMessageFormat = new LocalizableResourceString(nameof(Resources.TryPickAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TryPickDescription = new LocalizableResourceString(nameof(Resources.TryPickAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "OneOf";

        private static readonly DiagnosticDescriptor IsRule = new DiagnosticDescriptor(IsTypeDiagnosticId, IsTitle, IsMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: IsDescription);
        private static readonly DiagnosticDescriptor AsRule = new DiagnosticDescriptor(AsTypeDiagnosticId, AsTitle, AsMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: AsDescription);
        private static readonly DiagnosticDescriptor MapRule = new DiagnosticDescriptor(MapTypeDiagnosticId, MapTitle, MapMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: MapDescription);
        private static readonly DiagnosticDescriptor TryPickRule = new DiagnosticDescriptor(TryPickTypeDiagnosticId, TryPickTitle, TryPickMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: TryPickDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(IsRule, AsRule, MapRule, TryPickRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberExpression = (MemberAccessExpressionSyntax)context.Node;
            var text = memberExpression.Name.Identifier.Text;

            var startsWithIs = text.StartsWith("IsT");
            var startsWithAs = text.StartsWith("AsT");
            var startsWithMap = text.StartsWith("MapT");
            var startsWithTryPick = text.StartsWith("TryPickT");

            var substring = (startsWithIs || startsWithAs)
                ? 3
                : startsWithMap
                    ? 4
                    : 8;

            if ((startsWithIs || startsWithAs || startsWithMap || startsWithTryPick) && int.TryParse(text.Substring(substring), out _))
            {
                // Getting the expression part (the object on which the member is accessed)
                var expressionPart = memberExpression.Expression;

                // Getting the type of the expression
                var typeInfo = context.SemanticModel.GetTypeInfo(expressionPart).Type;

                if (typeInfo != null && typeInfo.Name == "OneOf")
                {
                    if (startsWithIs)
                    {
                        context.ReportDiagnostic(CreateDiagnostic(context, IsRule));
                    }
                    else if (startsWithAs)
                    {
                        context.ReportDiagnostic(CreateDiagnostic(context, AsRule));
                    }
                    else if (startsWithMap)
                    {
                        context.ReportDiagnostic(CreateDiagnostic(context, MapRule));
                    }
                    else
                    {
                        context.ReportDiagnostic(CreateDiagnostic(context, TryPickRule));
                    }
                }
            }
        }

        private static Diagnostic CreateDiagnostic(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor)
        {
            return Diagnostic.Create(descriptor, context.Node.GetLocation(), context.Node.ToString());
        }
    }
}
