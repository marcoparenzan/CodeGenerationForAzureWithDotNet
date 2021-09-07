using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MinimalApiGenerator;

public class ServiceSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> ClassesToPublish { get; } = new List<ClassDeclarationSyntax>();

    void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.Attribute))
        {
            var attribute = (AttributeSyntax)syntaxNode;
            if (attribute.Name.ToString() == "PublishHttp")
            {
                ClassesToPublish.Add((ClassDeclarationSyntax)syntaxNode.Parent.Parent);
            }
        }
    }
}
