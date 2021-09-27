//using System;
//using System.CommandLine;
//using System.CommandLine.Invocation;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//using Microsoft.DotNet.Interactive;
//using Microsoft.DotNet.Interactive.Commands;
//using Microsoft.DotNet.Interactive.CSharp;
//using Microsoft.DotNet.Interactive.Formatting;

//using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

//namespace CSVExtension
//{
//    public class CSVKernelExtension : IKernelExtension
//    {
//        public Task OnLoadAsync(Kernel kernel)
//        {
//            var loadCsvCommand = new Command("#!loadcsv", "Load and parse a CSV file")
//            {
//                new Option<string>(new[]{"-p","--path"},
//                                "The path of the file"),
//                new Option<string>(new[]{"-s","--separator"},
//                                "The separator char"),
//                new Option<string>(new[]{"-i","--itemTypeName"},
//                                "The separator char"),
//                new Option<string>(new[]{"-v","--variableName"},
//                                "The name of the variable")
//            };

//            loadCsvCommand.Handler = CommandHandler.Create(
//                async (string path, string separator, string itemTypeName, string variableName, KernelInvocationContext context) =>
//                {
//                    var sc = separator?[0] ?? ';';
//                    using (var reader = new StreamReader(path))
//                    {
//                        var fieldNames = reader.ReadLine().Split(sc);

//                        // define class
//                        var itemClassDef = @"
//public class " + itemTypeName + @"
//{" + string.Join("\r\n", fieldNames.Select(xx => $"\tpublic string {xx} {{ get; set; }}").ToArray()) + @"
//}
//                        ";
//                        context.DisplayStandardOut(itemClassDef.ToDisplayString());

//                        var itemClassDefCommand = new SubmitCode(itemClassDef);
//                        await context.HandlingKernel.SendAsync(itemClassDefCommand);

//                        // define list
//                        var declareItemListDef = $"var {variableName} = new System.Collections.Generic.List<{itemTypeName}>();";
//                        context.DisplayStandardOut(declareItemListDef.ToDisplayString());
//                        var declareItemListDefCommand = new SubmitCode(declareItemListDef);
//                        await context.HandlingKernel.SendAsync(declareItemListDefCommand);

//                        while (true)
//                        {
//                            var line = reader.ReadLine();
//                            if (string.IsNullOrWhiteSpace(line)) break;
//                            var fieldValues = line.Split(sc);
//                            var fields = fieldNames.Zip(fieldValues); // zip together name/value as name has to be repeated on each item class instance

//                            // add item
//                            var addItemListDef = variableName +".Add(new " + itemTypeName + " { " + string.Join(",", fields.Select(xx => xx.First + "= \"" + xx.Second + "\"").ToArray()) + "});";
//                            context.DisplayStandardOut(addItemListDef.ToDisplayString());
//                            var addItemListDefCommand = new SubmitCode(addItemListDef);
//                            await context.HandlingKernel.SendAsync(addItemListDefCommand);
//                        }
//                    }
//                });

//            kernel.AddDirective(loadCsvCommand);

//            if (KernelInvocationContext.Current is { } context)
//            {
//                PocketView view = div(
//                    code(nameof(CSVExtension)),
//                    " is loaded."
//                );

//                context.DisplayStandardOut(view.ToDisplayString());
//            }

//            return Task.CompletedTask;
//        }
//    }
//}