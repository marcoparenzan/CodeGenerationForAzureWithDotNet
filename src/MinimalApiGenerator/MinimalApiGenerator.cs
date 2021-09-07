using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MinimalApiGenerator;

[Generator]
public class MinimalApiGenerator : ISourceGenerator
{
    void ISourceGenerator.Execute(GeneratorExecutionContext context)
    {
        var sr = (ServiceSyntaxReceiver)context.SyntaxReceiver; // obtain the instance
        foreach (var classDecl in sr.ClassesToPublish)
        {
            foreach (MethodDeclarationSyntax methodDecl in classDecl.Members.Where(xx => xx.IsKind(SyntaxKind.MethodDeclaration)))
            {
                var generatedName = $"Service.{methodDecl.Identifier}.cs";

                var ns = GenerateMethodModule("MinimalApiCore", classDecl, methodDecl);

                var source = SourceText.From(ns.NormalizeWhitespace().ToString(), Encoding.UTF8);
                context.AddSource(generatedName, source);
            }
        }
    }

    void ISourceGenerator.Initialize(GeneratorInitializationContext context)
    {
        var syntaxReceiver = new ServiceSyntaxReceiver(); // local function, not member
        context.RegisterForSyntaxNotifications(() => syntaxReceiver);
    }

    NamespaceDeclarationSyntax GenerateMethodModule(string nsName, ClassDeclarationSyntax classDecl, MethodDeclarationSyntax methodDecl)
    {
        var ns = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.IdentifierName(nsName))
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Contract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Threading.Tasks"))
            )
            .AddMembers(
                // request
                SyntaxFactory.ClassDeclaration($"{methodDecl.Identifier}Request")
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
                )
                ,
                // response
                SyntaxFactory.ClassDeclaration($"{methodDecl.Identifier}Response")
                .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddMembers(
                    SyntaxFactory.PropertyDeclaration(methodDecl.ReturnType, "value")
                    .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(SyntaxFactory.AccessorList().AddAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    ))
                ),
                SyntaxFactory.ClassDeclaration($"{classDecl.Identifier}Service")
                .WithModifiers(
                    SyntaxFactory.TokenList()
                        .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                )
                .AddMembers(
                    SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.ParseTypeName("void")
                        //SyntaxFactory.IdentifierName("Task")
                        , $"Handle{methodDecl.Identifier}"
                    )
                    .WithParameterList(
                            SyntaxFactory.ParseParameterList($"({methodDecl.Identifier}Request request, {methodDecl.Identifier}Response response)")
                    )
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword) // ,
                        //SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
                    )
                    .AddBodyStatements(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("response"), SyntaxFactory.IdentifierName("value")),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("service"), SyntaxFactory.IdentifierName(methodDecl.Identifier))
                                )
                                .AddArgumentListArguments(
                                        methodDecl.ParameterList.Parameters.Select(xx => SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("request"), SyntaxFactory.IdentifierName(xx.Identifier)))).ToArray()
                                ))
                            )
                        )
                    )
            );

        return ns;
    }

}
