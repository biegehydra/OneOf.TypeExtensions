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

        private static readonly LocalizableString IsTitle = new LocalizableResourceString(nameof(Resources.IsAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString IsMessageFormat = new LocalizableResourceString(nameof(Resources.IsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString IsDescription = new LocalizableResourceString(nameof(Resources.IsAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString AsTitle = new LocalizableResourceString(nameof(Resources.AsAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsMessageFormat = new LocalizableResourceString(nameof(Resources.AsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsDescription = new LocalizableResourceString(nameof(Resources.AsAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "OneOf";

        private static readonly DiagnosticDescriptor IsRule = new DiagnosticDescriptor(IsTypeDiagnosticId, IsTitle, IsMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: IsDescription);
        private static readonly DiagnosticDescriptor AsRule = new DiagnosticDescriptor(AsTypeDiagnosticId, AsTitle, AsMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: AsDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(IsRule, AsRule);

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

            if ((startsWithIs || startsWithAs) && int.TryParse(text.Substring(3), out _))
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
                    else
                    {
                        context.ReportDiagnostic(CreateDiagnostic(context, AsRule));
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
