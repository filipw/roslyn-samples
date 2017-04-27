using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace EmptyDestructor
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyDestructorCodeFixProvider)), Shared]
    public class EmptyDestructorCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove empty destructor";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(EmptyDestructorAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<DestructorDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c =>
                    {
                        return Task.FromResult(context.Document.WithSyntaxRoot(root.RemoveNode(declaration, SyntaxRemoveOptions.AddElasticMarker)));
                    },
                    equivalenceKey: title),
                diagnostic);
        }
    }
}