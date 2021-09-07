using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LibraryGenerator;

[Generator]
public class FunctionGenerator : ISourceGenerator
{
    void ISourceGenerator.Initialize(GeneratorInitializationContext context)
    {
    }

    void ISourceGenerator.Execute(GeneratorExecutionContext context)
    {
        var className = File.ReadAllText(context.AdditionalFiles[0].Path);
        var sourceText = @$"
    public class {className}
    {{
    }}
";
        var demoCode = SourceText.From(sourceText, Encoding.UTF8);
        context.AddSource("demo.generated.cs", demoCode);
    }
}
