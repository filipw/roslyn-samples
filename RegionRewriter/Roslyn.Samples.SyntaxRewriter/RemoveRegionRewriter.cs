using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn.Samples.SyntaxRewriter
{
    public class RemoveRegionRewriter : CSharpSyntaxRewriter
    {
        public RemoveRegionRewriter()
            : base(visitIntoStructuredTrivia: true)
        {
        }

        public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
        {
            return SyntaxFactory.SkippedTokensTrivia();
        }

        public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
        {
            return SyntaxFactory.SkippedTokensTrivia();
        }
    }
}