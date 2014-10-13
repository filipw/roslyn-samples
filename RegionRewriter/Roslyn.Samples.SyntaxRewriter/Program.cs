using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslyn.Samples.SyntaxRewriter
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadAllText("Code.cs");

            Console.WriteLine("Before parsing:");
            Console.WriteLine(code);
            Console.WriteLine();

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var syntaxRewriter = new RemoveRegionRewriter();
            var result = syntaxRewriter.Visit(syntaxTree.GetRoot()).NormalizeWhitespace();

            Console.WriteLine("After parsing:");
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
