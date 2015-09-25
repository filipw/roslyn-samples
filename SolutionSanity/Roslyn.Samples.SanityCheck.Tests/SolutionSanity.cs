using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Should;
using Xunit;

namespace Roslyn.Samples.SanityCheck.Tests
{
    public class SolutionSanity
    {
        private readonly List<Document> _documents;

        public SolutionSanity()
        {
            var slnPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Roslyn.Samples.SanityCheck.sln"));

            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(slnPath).Result;

            _documents = new List<Document>();

            foreach (var projectId in solution.ProjectIds)
            {
                var project = solution.GetProject(projectId);
                foreach (var documentId in project.DocumentIds)
                {
                    var document = solution.GetDocument(documentId);
                    if (document.SupportsSyntaxTree)
                    {
                        _documents.Add(document);
                    }
                }
            }
        }

        [Fact]
        public void Interfaces_ShouldBePrefixedWithI()
        {
            var interfaces = _documents.SelectMany(x => x.GetSyntaxRootAsync().Result.DescendantNodes().OfType<InterfaceDeclarationSyntax>()).ToList();
            interfaces.All(x => x.Identifier.ToString().StartsWith("I")).ShouldBeTrue();
        }

        [Fact]
        public async Task ICalculationResult_ShouldNotHavePublicSetters()
        {
            foreach (var doc in _documents)
            {
                var root = await doc.GetSyntaxRootAsync();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                if (classes.Any())
                {
                    var semanticModel = await doc.GetSemanticModelAsync();
                    foreach (var c in classes)
                    {
                        var classSymbol = semanticModel.GetDeclaredSymbol(c) as ITypeSymbol;
                        if (classSymbol != null)
                        {
                            if (classSymbol.AllInterfaces.Any(x => x.Name == "ICalculationResult"))
                            {
                                var properties = classSymbol.GetMembers().Where(m => m.Kind == SymbolKind.Property).Cast<IPropertySymbol>();
                                if (properties.Any())
                                {
                                    properties.All(x => x.IsReadOnly || x.SetMethod.DeclaredAccessibility < Accessibility.Public).ShouldBeTrue();
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public void Regions_AreNotAllowed()
        {
            var regions = _documents.SelectMany(x => 
                x.GetSyntaxRootAsync().Result.DescendantNodesAndTokens().
                Where(n => n.HasLeadingTrivia).SelectMany(n => n.GetLeadingTrivia().
                    Where(t => t.Kind() == SyntaxKind.RegionDirectiveTrivia))).ToList();

            regions.ShouldBeEmpty();
        }
    }
}
