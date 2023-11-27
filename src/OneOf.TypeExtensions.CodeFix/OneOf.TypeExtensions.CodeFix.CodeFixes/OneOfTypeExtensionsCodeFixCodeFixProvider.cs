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
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OneOfTypeExtensionsCodeFixAnalyzer.IsTypeDiagnosticId, OneOfTypeExtensionsCodeFixAnalyzer.AsTypeDiagnosticId, OneOfTypeExtensionsCodeFixAnalyzer.MapTypeDiagnosticId, OneOfTypeExtensionsCodeFixAnalyzer.TryPickTypeDiagnosticId);

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
                                createChangedDocument: c => ConvertIsTAsync(context.Document, declaration, c),
                                equivalenceKey: nameof(CodeFixResources.IsTypeTitle)),
                            diagnostic);
                        break;

                    case OneOfTypeExtensionsCodeFixAnalyzer.AsTypeDiagnosticId:
                        // Register a code action for the 'AsType' diagnostic
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: CodeFixResources.AsTypeTitle,
                                createChangedDocument: c => ConvertAsTAsync(context.Document, declaration, c),
                                equivalenceKey: nameof(CodeFixResources.AsTypeTitle)),
                            diagnostic);
                        break;
                    case OneOfTypeExtensionsCodeFixAnalyzer.MapTypeDiagnosticId:
                        // Register a code action for the 'AsType' diagnostic
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: CodeFixResources.MapTypeTitle,
                                createChangedDocument: c => ConvertMapTAsync(context.Document, declaration, c),
                                equivalenceKey: nameof(CodeFixResources.MapTypeTitle)),
                            diagnostic);
                        break;
                    case OneOfTypeExtensionsCodeFixAnalyzer.TryPickTypeDiagnosticId:
                        // Register a code action for the 'AsType' diagnostic
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: CodeFixResources.TryPickTypeTitle,
                                createChangedDocument: c => ConvertTryPickTAsync(context.Document, declaration, c),
                                equivalenceKey: nameof(CodeFixResources.TryPickTypeTitle)),
                            diagnostic);
                        break;
                }
            }
        }

        private async Task<Document> ConvertIsTAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
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

        private async Task<Document> ConvertAsTAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
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

        private async Task<Document> ConvertMapTAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
        {
            var text = memberAccessExpression.Name.Identifier.Text;

            if (!text.StartsWith("MapT") || !int.TryParse(text.Substring(4), out var index))
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type;

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol ||
                namedTypeSymbol.TypeArguments.Length < index + 1)
            {
                return document;
            }

            var typeArgument = HelperExtensions.GetTypeArgument(namedTypeSymbol.TypeArguments[index]);
            var newMethodName = $"Map{typeArgument.ReadableName}";

            // Find the InvocationExpressionSyntax that the memberAccessExpression is a part of
            var invocationExpression = memberAccessExpression.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocationExpression == null)
            {
                // If the memberAccessExpression is not part of an invocation, we cannot proceed
                return document;
            }

            // Retain the original expression and append the new method name
            var newMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpression.Expression,
                SyntaxFactory.IdentifierName(newMethodName));

            // Create a new method invocation expression with the new member access
            var newMethodInvocation = SyntaxFactory.InvocationExpression(
                newMemberAccess,
                invocationExpression.ArgumentList);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root == null) return document;
            var newRoot = root.ReplaceNode(invocationExpression, newMethodInvocation);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> ConvertTryPickTAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
        {
            var text = memberAccessExpression.Name.Identifier.Text;

            if (!text.StartsWith("TryPickT") || !int.TryParse(text.Substring(8), out var index))
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type;

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol ||
                namedTypeSymbol.TypeArguments.Length < index + 1)
            {
                return document;
            }

            var typeArgument = HelperExtensions.GetTypeArgument(namedTypeSymbol.TypeArguments[index]);
            var newMethodName = $"TryPick{typeArgument.ReadableName}";

            // Find the InvocationExpressionSyntax that the memberAccessExpression is a part of
            var invocationExpression = memberAccessExpression.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocationExpression == null)
            {
                // If the memberAccessExpression is not part of an invocation, we cannot proceed
                return document;
            }

            // Retain the original expression and append the new method name
            var newMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpression.Expression,
                SyntaxFactory.IdentifierName(newMethodName));

            // Create a new method invocation expression with the new member access
            var newMethodInvocation = SyntaxFactory.InvocationExpression(
                newMemberAccess,
                invocationExpression.ArgumentList);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root == null) return document;
            var newRoot = root.ReplaceNode(invocationExpression, newMethodInvocation);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
