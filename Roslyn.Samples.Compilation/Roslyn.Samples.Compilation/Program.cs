using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslyn.Samples.Compilation
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var code = File.ReadAllText("Code.cs");

            var tree = SyntaxFactory.ParseSyntaxTree(code);

            var compilation = CSharpCompilation.Create(
                "test.dll",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] {tree},
                references: new[]
                {
                    new MetadataFileReference(typeof (object).Assembly.Location),
                });

            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                if (compileResult.Success)
                {
                    var compiledAssembly = Assembly.Load(stream.GetBuffer());
                    var program = compiledAssembly.GetType("Foo");
                    var entry = program.GetMethod("Bar");
                    entry.Invoke(null, null);
                }
                else
                {
                    foreach (var diagnostic in compileResult.Diagnostics)
                    {
                        Console.WriteLine(diagnostic.ToString());
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
