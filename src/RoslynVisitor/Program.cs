using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;

var text = File.ReadAllText(@"..\..\..\..\Contract\IService.cs");
text = @"
   [Description("")]
    public string XXX {get; set;}
";

var tree = CSharpSyntaxTree.ParseText(text);
//var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(@"..\..\..\..\ServiceHostApp\Service.DoSomething.cs"));

Console.WriteLine(tree.ToString());

Visit(tree.GetRoot(), System.Console.Out);

Console.ReadLine();

void Visit(SyntaxNode node, TextWriter writer, int level = 0)
{
    switch (node.Kind())
    {
        case SyntaxKind.NamespaceDeclaration:
            var ns = (NamespaceDeclarationSyntax)node;
            writer.WriteLine($"{"".PadRight(level, ' ')}{node.Kind()}({ns.Name})");
            SubVisit(node, writer, level);
            break;
        case SyntaxKind.UsingDirective:
            var ud = (UsingDirectiveSyntax)node;
            writer.WriteLine($"{"".PadRight(level, ' ')}{node.Kind()}({ud.Name})");
            break;
        case SyntaxKind.IdentifierName:
            var ins = (IdentifierNameSyntax)node;
            writer.WriteLine($"{"".PadRight(level, ' ')}{node.Kind()}({ins.Identifier})");
            break;
        default:
            writer.WriteLine($"{"".PadRight(level, ' ')}{node.Kind()}");
            SubVisit(node, writer, level);
            break;
    }
}

void SubVisit(SyntaxNode node, TextWriter writer, int level)
{
    foreach (var childNode in node.ChildNodes())
        Visit(childNode, writer, level + 1);
}
