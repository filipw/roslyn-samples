using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Should;
using Xunit;

namespace Roslyn.Samples.SanityCheck.Tests
{
    public class SolutionSanity
    {
        private readonly List<SyntaxNode> _documents;

        public SolutionSanity()
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(@"C:\Users\filipw\Documents\dev\roslyn-samples\SolutionSanity\Roslyn.Samples.SanityCheck.sln").Result;

            _documents = new List<SyntaxNode>();

            foreach (var projectId in solution.ProjectIds)
            {
                var project = solution.GetProject(projectId);
                foreach (var documentId in project.DocumentIds)
                {
                    var document = solution.GetDocument(documentId);
                    if (document.SupportsSyntaxTree)
                    {
                        var root = document.GetSyntaxRootAsync().Result;
                        _documents.Add(root);
                    }
                }
            }
        }

        [Fact]
        public void Interfaces_ShouldBePrefixedWithI()
        {
            var interfaces = _documents.SelectMany(x => x.DescendantNodes().OfType<InterfaceDeclarationSyntax>()).ToList();
            interfaces.All(x => x.Identifier.ToString().StartsWith("I")).ShouldBeTrue();
        }
    }
}
