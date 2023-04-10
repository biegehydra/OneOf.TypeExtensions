using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OneOf.TypeExtensions.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OneOfTypeExtensionsCodeFixCodeFixProvider)), Shared]
    public class OneOfTypeExtensionsCodeFixCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OneOfTypeExtensionsCodeFixAnalyzer.IsTypeDiagnosticId, OneOfTypeExtensionsCodeFixAnalyzer.AsTypeDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                // Find the type declaration identified by the diagnostic.
                var declaration = root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();

                // Ensure declaration is not null
                if (declaration == null)
                    continue;

                switch (diagnostic.Id)
                {
                    case OneOfTypeExtensionsCodeFixAnalyzer.IsTypeDiagnosticId:
                        // Register a code action for the 'IsType' diagnostic
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: CodeFixResources.IsTypeTitle,
                                createChangedDocument: c => ConvertIsTNumberToExtensionMethodCallAsync(context.Document, declaration, c),
                                equivalenceKey: nameof(CodeFixResources.IsTypeTitle)),
                            diagnostic);
                        break;

                    case OneOfTypeExtensionsCodeFixAnalyzer.AsTypeDiagnosticId:
                        // Register a code action for the 'AsType' diagnostic
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: CodeFixResources.AsTypeTitle,
                                createChangedDocument: c => ConvertAsTNumberToExtensionMethodCallAsync(context.Document, declaration, c),
                                equivalenceKey: nameof(CodeFixResources.AsTypeTitle)),
                            diagnostic);
                        break;
                }
            }
        }

        private async Task<Document> ConvertIsTNumberToExtensionMethodCallAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
        {
            var text = memberAccessExpression.Name.Identifier.Text;

            if (!text.StartsWith("IsT") || !int.TryParse(text.Substring(3), out var index))
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type;

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol ||
                namedTypeSymbol.TypeArguments.Length < index + 1)
            {
                return document;
            }

            var typeArgument = HelperExtensions.GetTypeArgument(namedTypeSymbol.TypeArguments[index]);
            var newMethodName = $"Is{typeArgument.ReadableName}";

            // Retain the original expression and append the new method name
            var newMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpression.Expression,
                SyntaxFactory.IdentifierName(newMethodName));

            // Create a new method invocation expression with the new member access
            var newMethodInvocation = SyntaxFactory.InvocationExpression(
                newMemberAccess,
                SyntaxFactory.ArgumentList());

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root == null) return document;
            var newRoot = root.ReplaceNode(memberAccessExpression, newMethodInvocation);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> ConvertAsTNumberToExtensionMethodCallAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
        {
            var text = memberAccessExpression.Name.Identifier.Text;

            if (!text.StartsWith("AsT") || !int.TryParse(text.Substring(3), out var index))
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type;

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol ||
                namedTypeSymbol.TypeArguments.Length < index + 1)
            {
                return document;
            }

            var typeArgument = HelperExtensions.GetTypeArgument(namedTypeSymbol.TypeArguments[index]);
            var newMethodName = $"As{typeArgument.ReadableName}";

            // Retain the original expression and append the new method name
            var newMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpression.Expression,
                SyntaxFactory.IdentifierName(newMethodName));

            // Create a new method invocation expression with the new member access
            var newMethodInvocation = SyntaxFactory.InvocationExpression(
                newMemberAccess,
                SyntaxFactory.ArgumentList());

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root == null) return document;
            var newRoot = root.ReplaceNode(memberAccessExpression, newMethodInvocation);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
