using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(@"..\..\..\..\ServiceHostAppGenerated\ServiceImpl.cs"));

List<ClassDeclarationSyntax> ClassesToPublish = new List<ClassDeclarationSyntax>();

Visit(tree.GetRoot());

foreach (var classDecl in ClassesToPublish)
{
    foreach (MethodDeclarationSyntax methodDecl in classDecl.Members.Where(xx => xx.IsKind(SyntaxKind.MethodDeclaration)))
    {
        var ns = GenerateMethodModule(classDecl, methodDecl);
        Console.WriteLine(ns.NormalizeWhitespace());
    }
}

NamespaceDeclarationSyntax GenerateMethodModule(ClassDeclarationSyntax classDecl, MethodDeclarationSyntax methodDecl)
{
    var ns = SyntaxFactory
        .NamespaceDeclaration(SyntaxFactory.IdentifierName("ServiceHostAppGenerated"))
        .AddUsings(
            MyUsing("Contract"),
            MyUsing("Microsoft.Azure.Functions.Worker"),
            MyUsing("Microsoft.Azure.Functions.Worker.Http"),
            MyUsing("Microsoft.Extensions.Logging"),
            MyUsing("System.Threading.Tasks")
        )
        .AddMembers(
            // request
            (MemberDeclarationSyntax)AddRequestClass(methodDecl)
            ,
            // response
            (MemberDeclarationSyntax)AddResponseClass(methodDecl),
            SyntaxFactory.ClassDeclaration($"{classDecl.Identifier}Service")
            .WithModifiers(
                SyntaxFactory.TokenList()
                    .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            )
            .AddMembers(
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName("Task").AddTypeArgumentListArguments(
                        SyntaxFactory.IdentifierName($"HttpResponseData")
                    )
                    , methodDecl.Identifier
                )
                .WithParameterList(
                        SyntaxFactory.ParseParameterList("([HttpTrigger(AuthorizationLevel.Function, \"post\")] HttpRequestData requestData)")
                )
                .AddAttributeLists(
                    SyntaxFactory.AttributeList()
                        .AddAttributes(
                            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Function"))
                                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(methodDecl.Identifier.ToString()))))
                        )
                )
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
                )
                .AddBodyStatements(
                    SyntaxFactory.ParseStatement($"var request = await Request<{methodDecl.Identifier}Request>(requestData);"),
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(methodDecl.ReturnType.ToString()))
                            .AddVariables(
                                SyntaxFactory.VariableDeclarator("value")
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("service"), SyntaxFactory.IdentifierName(methodDecl.Identifier))
                                        )
                                        .AddArgumentListArguments(
                                                methodDecl.ParameterList.Parameters.Select(xx => SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("request"), SyntaxFactory.IdentifierName(xx.Identifier)))).ToArray()
                                        ))
                                )
                            )
                    ),
                    SyntaxFactory.ParseStatement(@$" var response = new {methodDecl.Identifier}Response
                        {{
                            value = value
                        }};"),
                    SyntaxFactory.ParseStatement("return await Response(requestData, response);")
                )
            )
        );

    return ns;
}

Console.ReadLine();

void Visit(SyntaxNode syntaxNode)
{
    if (syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.Attribute))
    {
        var attribute = (AttributeSyntax)syntaxNode;
        if (attribute.Name.ToString() == "PublishHttp")
        {
            ClassesToPublish.Add((ClassDeclarationSyntax)syntaxNode.Parent.Parent);
            return;
        }
    }
    foreach (var childNode in syntaxNode.ChildNodes())
        Visit(childNode);
}

UsingDirectiveSyntax MyUsing(string name) => SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(name));

ClassDeclarationSyntax AddRequestClass(MethodDeclarationSyntax methodDecl)
{
    return SyntaxFactory.ClassDeclaration($"{methodDecl.Identifier}Request")
                .WithModifiers(SyntaxFactory.TokenList()
                    .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                )
                .AddMembers(
                    methodDecl.ParameterList.Parameters.Select(xx =>
                        SyntaxFactory.PropertyDeclaration(xx.Type, xx.Identifier.ToString())
                        .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .WithAccessorList(SyntaxFactory.AccessorList().AddAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        ))
                    ).ToArray()
                );
}

ClassDeclarationSyntax AddResponseClass(MethodDeclarationSyntax methodDecl, string returnVariableName = "value")
{
    return SyntaxFactory.ClassDeclaration($"{methodDecl.Identifier}Response")
                .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddMembers(
                    SyntaxFactory.PropertyDeclaration(methodDecl.ReturnType, returnVariableName)
                    .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(SyntaxFactory.AccessorList().AddAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    ))
                );
}