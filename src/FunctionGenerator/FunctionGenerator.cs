using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FunctionGenerator;

[Generator]
public class FunctionGenerator : ISourceGenerator
{
    void ISourceGenerator.Execute(GeneratorExecutionContext context)
    {
        var sr = (ServiceSyntaxReceiver)context.SyntaxReceiver; // obtain the instance

        foreach (var classDecl in sr.ClassesToPublish)
        {
            foreach (MethodDeclarationSyntax methodDecl in classDecl.Members.Where(xx => xx.IsKind(SyntaxKind.MethodDeclaration)))
            {
                var generatedName = $"Service.{methodDecl.Identifier}.cs";

                var ns = GenerateMethodModule(classDecl, methodDecl);

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

    NamespaceDeclarationSyntax GenerateMethodModule(ClassDeclarationSyntax classDecl, MethodDeclarationSyntax methodDecl)
    {
        var ns = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.IdentifierName("ServiceHostAppGenerated"))
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Contract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.Azure.Functions.Worker")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.Azure.Functions.Worker.Http")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.Extensions.Logging")),
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

}
