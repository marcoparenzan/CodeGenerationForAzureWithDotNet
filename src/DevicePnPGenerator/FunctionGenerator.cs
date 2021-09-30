using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class FunctionGenerator : ISourceGenerator
{
    static FunctionGenerator()
    {
    }
    void ISourceGenerator.Execute(GeneratorExecutionContext context)
    {
        var parser = new ModelParser();
        var text = File.ReadAllText(context.AdditionalFiles.First().Path);
        var jsonDoc = JsonDocument.Parse(text);
        var types = jsonDoc.RootElement.EnumerateArray().Select(xx => xx.ToString()).ToArray();
        var entities = parser.ParseAsync(types).Result;

        var interfaceInfo = entities.Where(xx => xx.Value is DTInterfaceInfo).Select(xx => xx.Value).Cast<DTInterfaceInfo>().ToArray();
        var name = interfaceInfo.Last().Id.Labels.Last();
        var temetryInfos = entities.Where(xx => xx.Value is DTTelemetryInfo).Select(xx => xx.Value).Cast<DTTelemetryInfo>().ToArray();

        var itemClassDecl = GenerateCode(name, temetryInfos);

        var source = SourceText.From(itemClassDecl.NormalizeWhitespace().ToString(), Encoding.UTF8);
        context.AddSource($"{name}.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    private static NamespaceDeclarationSyntax GenerateCode(string name, DTTelemetryInfo[] temetryInfos)
    {   return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Models"))
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel"))).AddMembers(
                 SyntaxFactory.ClassDeclaration($"{name}")
                    .WithModifiers(SyntaxFactory.TokenList()
                        .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                    )
                    .AddMembers(
                        temetryInfos.Select(xx =>
                            SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(TypeOf(xx.Schema)), xx.Name)
                            .AddAttributeLists(
                                SyntaxFactory.AttributeList()
                                .AddAttributes(
                                    SyntaxFactory.Attribute(
                                        SyntaxFactory.IdentifierName("Description")
                                    )
                                    .AddArgumentListArguments(
                                        SyntaxFactory.AttributeArgument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(xx.Id.ToString())
                                            )
                                        )
                                    )
                                )
                            )
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                            )
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            )
                        ).ToArray()
                    ));
    }

    private static string TypeOf(DTSchemaInfo schema)
    {
        if (schema is DTEnumInfo enumInfo)
        {
            return TypeOf(enumInfo.ValueSchema);
        }
        else
        {
            switch (schema.EntityKind)
            {
                case DTEntityKind.Integer:
                    return "int";
                default:
                    return schema.EntityKind.ToString();
            }
        }
    }
}
