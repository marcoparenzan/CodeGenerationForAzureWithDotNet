using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SwaggerProxyGenerator.Models;
using System.Text.Json;

var httpClient = new HttpClient();
var swaggerJson = await httpClient.GetStringAsync("https://localhost:5001/swagger/v1/swagger.json");
var swaggerSchema = JsonSerializer.Deserialize<SwaggerSchema>(swaggerJson);

// messages

foreach (var cs in swaggerSchema.Components.Schemas)
{
    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("Models"))
    .AddMembers(
        SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(cs.Key))
        .AddModifiers(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.PartialKeyword)
        )
        .AddMembers(cs.Value.Properties.Select(xx => Map(xx.Key, xx.Value)).ToArray())
    );
    File.WriteAllText(@$"..\..\..\..\SwaggerClient\{cs.Key}.cs", ns.NormalizeWhitespace().ToString());
}

// paths

foreach (var p in swaggerSchema.Paths)
{
    var name = NameOf(p.Key);
    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("Models"))
    .AddMembers(
        SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(name))
        .AddBaseListTypes(
            SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ProxyBase"))
        )
        .AddModifiers(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.PartialKeyword)
        )
        .AddMembers(items: Map1(name, p.Key, p.Value["post"]))
    );
    File.WriteAllText(@$"..\..\..\..\SwaggerClient\{name}.cs", ns.NormalizeWhitespace().ToString());
}

MemberDeclarationSyntax Map1(string name, string path, SwaggerSchemaPath value)
{
    var xx = default(MemberDeclarationSyntax);

    xx = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseName($"{name}Response"), $"Execute")
        .AddModifiers(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)
        )
        .AddParameterListParameters(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
            .WithType(SyntaxFactory.ParseTypeName($"{name}Request"))
        )
        .AddBodyStatements(
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, 
                        SyntaxFactory.ThisExpression(), 
                        SyntaxFactory.GenericName("Invoke")
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.ParseName($"{name}Request"),
                                SyntaxFactory.ParseName($"{name}Response")
                            )
                    )
                )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,SyntaxFactory.Literal(path))
                    ),
                    SyntaxFactory.Argument(
                        SyntaxFactory.IdentifierName("request")
                    )
                )
            )
        )
    ;

    return xx;
}

string NameOf(string key) => key.Split('/').Last();

MemberDeclarationSyntax Map(string propertyName, ComponentSchemaDescProperty propertyDesc)=> SyntaxFactory.PropertyDeclaration(
    TypeOf(propertyDesc.Type),
    propertyName
)
.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
.AddAccessorListAccessors(
    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
)
;

TypeSyntax TypeOf(string type)
{
    switch (type)
    {
        case "integer":
            return SyntaxFactory.ParseTypeName("int");
        default:
            return SyntaxFactory.ParseTypeName(type);
    }
}