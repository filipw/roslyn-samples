using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EmptyDestructor
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyDestructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EmptyDestructor";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Performance";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(ctx =>
            {
                var destructor = (DestructorDeclarationSyntax)ctx.Node;
                if (destructor.Body == null || destructor.Body.DescendantNodes().Any()) return;

                ctx.ReportDiagnostic(Diagnostic.Create(Rule, destructor.GetLocation(), destructor.Identifier.Text));
            }, SyntaxKind.DestructorDeclaration);
        }
    }
}
