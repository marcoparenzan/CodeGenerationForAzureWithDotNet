using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text.Json;

var parser = new ModelParser();
var text = File.ReadAllText("Solar Panel v1.1.json");
var jsonDoc = JsonDocument.Parse(text);
var xx = jsonDoc.RootElement.EnumerateArray().Select(xx => xx.ToString()).ToArray();
var entities = await parser.ParseAsync(xx);

var name = entities.Values.First().DisplayName;

var itemClassDecl = SyntaxFactory.ClassDeclaration($"{name}")
    .WithModifiers(SyntaxFactory.TokenList()
        .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
    )
    .AddMembers(
        entities.Skip(1).Select(xx =>
            SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), xx.Value.Id.ToString())
            .AddAttributeLists(
                SyntaxFactory.AttributeList()
                .AddAttributes(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("DescriptionAttribute")
                    )
                    .AddArgumentListArguments(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(xx.Value.Id.ToString())
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
    )
;

Console.WriteLine(itemClassDecl.NormalizeWhitespace());

Console.ReadLine();