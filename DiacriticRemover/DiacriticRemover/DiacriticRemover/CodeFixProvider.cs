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
using System.Text;

namespace DiacriticRemover
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DiacriticRemoverCodeFixProvider)), Shared]
    public class DiacriticRemoverCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove diacritics";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiacriticRemoverAnalyzer.DiagnosticId); }
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
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => Rename(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> Rename(Document document, BaseTypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var newName = CleanUpText(typeDecl.Identifier.Text);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, document.Project.Solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
            return newSolution;
        }

        private static String CleanUpText(String germanText)
        {
            var map = new Dictionary<char, string>() {
              { 'ä', "ae" },
              { 'ö', "oe" },
              { 'ü', "ue" },
              { 'Ä', "Ae" },
              { 'Ö', "Oe" },
              { 'Ü', "Ue" },
              { 'ß', "ss" }
            };

            return germanText.ToCharArray().Aggregate(
                          new StringBuilder(),
                          (sb, c) =>
                          {
                              if (map.TryGetValue(c, out string r))
                                  return sb.Append(r);
                              else
                                  return sb.Append(c);
                          }).ToString();
        }
    }
}