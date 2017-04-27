using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Runtime.CompilerServices;

namespace ConfigureAwaitRefactoring
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ConfigureAwaitRefactoringCodeRefactoringProvider)), Shared]
    internal class ConfigureAwaitRefactoringCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var awaitExpr = node as AwaitExpressionSyntax;
            if (awaitExpr == null) return;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var symbol = semanticModel.GetSymbolInfo(awaitExpr.Expression, context.CancellationToken).Symbol as IMethodSymbol;
            if (symbol.ReturnType.MetadataName == nameof(ConfiguredTaskAwaitable)) return;

            var action = CodeAction.Create("Add ConfigureAwait(false)", c => AddConfigureAwait(context.Document, awaitExpr.Expression, c));
            context.RegisterRefactoring(action);
        }

        private async Task<Document> AddConfigureAwait(Document document, ExpressionSyntax awaitedExpression, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            var generator = editor.Generator;

            var newInvocation = generator.InvocationExpression(
                generator.MemberAccessExpression(awaitedExpression.WithoutTrivia(), "ConfigureAwait"), 
                generator.FalseLiteralExpression());

            newInvocation = newInvocation.WithLeadingTrivia(awaitedExpression.GetLeadingTrivia()).WithTrailingTrivia(awaitedExpression.GetTrailingTrivia());
            editor.ReplaceNode(awaitedExpression, newInvocation);

            return editor.GetChangedDocument();
        }
    }
}