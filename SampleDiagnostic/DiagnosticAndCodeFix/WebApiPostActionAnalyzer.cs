using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiagnosticAndCodeFix
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class WebApiPostActionAnalyzer : ISymbolAnalyzer
    {
        public const string DiagnosticId = "WebApiPostActionDiagnosticAndFix";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Consider returning 201", "Do not use void with Post actions", "Naming", DiagnosticSeverity.Warning, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public ImmutableArray<SymbolKind> SymbolKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(SymbolKind.Method);
            }
        }

        public void AnalyzeSymbol(ISymbol symbol, Compilation compilation, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            var methodSymbol = (IMethodSymbol)symbol;
            var httpControllerInterfaceSymbol = compilation.GetTypeByMetadataName("System.Web.Http.Controllers.IHttpController");

            if (methodSymbol.ContainingType.AllInterfaces.Any(x => x.MetadataName == httpControllerInterfaceSymbol.MetadataName))
            {
                var postAttributeSymbol = compilation.GetTypeByMetadataName("System.Web.Http.HttpPostAttribute");

                if (methodSymbol.ReturnsVoid && (methodSymbol.MetadataName.ToLowerInvariant().StartsWith("post") || methodSymbol.GetAttributes().Any(x => x.AttributeClass.MetadataName == postAttributeSymbol.MetadataName)))
                {
                    var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
                    addDiagnostic(diagnostic);
                }
            }
        }
    }
}
