using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CSVExtension
{
    public class CSVKernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            var loadCsvCommand = new Command("#!loadcsv", "Load and parse a CSV file")
            {
                new Option<string>(new[]{"-p","--path"},
                                "The path of the file"),
                new Option<string>(new[]{"-s","--separator"},
                                "The separator char"),
                new Option<string>(new[]{"-i","--itemTypeName"},
                                "The separator char"),
                new Option<string>(new[]{"-v","--variableName"},
                                "The name of the variable")
            };

            loadCsvCommand.Handler = CommandHandler.Create(
                async (string path, string separator, string itemTypeName, string variableName, KernelInvocationContext context) =>
                {
                    var sc = separator?[0] ?? ';';
                    using (var reader = new StreamReader(path))
                    {
                        var fieldNames = reader.ReadLine().Split(sc);

                        // define class

                        var itemClassDecl = SyntaxFactory.ClassDeclaration($"{itemTypeName}")
                            .WithModifiers(SyntaxFactory.TokenList()
                                .Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            )
                            .AddMembers(
                                fieldNames.Select(xx =>
                                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), xx)
                                    .WithModifiers(SyntaxFactory.TokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithAccessorList(SyntaxFactory.AccessorList().AddAccessors(
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    ))
                                ).ToArray()
                            );

                        context.DisplayStandardOut(itemClassDecl.NormalizeWhitespace().ToString());

                        // send class declaration code
                        var itemClassDefCommand = new SubmitCode(itemClassDecl.NormalizeWhitespace().ToString());
                        await context.HandlingKernel.SendAsync(itemClassDefCommand);

                        // define list

                        var declareItemListDecl = SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                            .AddVariables(
                                SyntaxFactory.VariableDeclarator(variableName)
                                .WithInitializer(SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.ParseName("System.Collections.Generic"),
                                            SyntaxFactory.GenericName("List")
                                                .AddTypeArgumentListArguments(SyntaxFactory.IdentifierName(itemTypeName))
                                        )
                                    )
                                    .AddArgumentListArguments()
                                ))
                            )
                        );

                        context.DisplayStandardOut(declareItemListDecl.NormalizeWhitespace().ToString());
                        var declareItemListDefCommand = new SubmitCode(declareItemListDecl.NormalizeWhitespace().ToString());
                        await context.HandlingKernel.SendAsync(declareItemListDefCommand);

                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line)) break;
                            var fieldValues = line.Split(sc);
                            var fields = fieldNames.Zip(fieldValues); // zip together name/value as name has to be repeated on each item class instance

                            // add item

                            var addItemListDecl = SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(variableName),
                                        SyntaxFactory.IdentifierName("Add")
                                    )
                                )
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName(itemTypeName)
                                        )
                                        .WithInitializer(
                                            SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                                            .AddExpressions(
                                                fields.Select(xx => SyntaxFactory.AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    SyntaxFactory.IdentifierName(xx.First),
                                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(xx.Second))
                                                )).ToArray()
                                            )
                                        )
                                    )
                                )
                            );

                            context.DisplayStandardOut(addItemListDecl.NormalizeWhitespace().ToString());
                            var addItemListDefCommand = new SubmitCode(addItemListDecl.NormalizeWhitespace().ToString());
                            await context.HandlingKernel.SendAsync(addItemListDefCommand);
                        }
                    }
                });

            kernel.AddDirective(loadCsvCommand);

            if (KernelInvocationContext.Current is { } context)
            {
                PocketView view = div(
                    code(nameof(CSVExtension)),
                    " is loaded."
                );

                context.DisplayStandardOut(view.ToDisplayString());
            }

            return Task.CompletedTask;
        }
    }
}