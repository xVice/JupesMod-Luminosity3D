using System;
using System.IO;
using System.Reflection;
using Luminosity3DPAK;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using Luminosity3D.LuminosityPackageLoader;
using System.Reflection.Emit;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Luminosity3D.EntityComponentSystem;

namespace Luminosity3D.Utils
{
    class VariableAssignmentWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel semanticModel;
        private readonly DataField dataField;

        public VariableAssignmentWalker(SemanticModel semanticModel, DataField dataField)
        {
            this.semanticModel = semanticModel;
            this.dataField = dataField;
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (node.Left is IdentifierNameSyntax identifier)
            {
                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol as IFieldSymbol;
                if (symbol != null)
                {
                    // Extract the variable name and its value
                    var variableName = symbol.Name;
                    var variableValue = semanticModel.GetConstantValue(node.Right);

                    if (variableValue.HasValue)
                    {
                        dataField.Set(variableName, variableValue.Value);
                    }
                }
            }

            base.VisitAssignmentExpression(node);
        }
    }


    public class RoslynCodeLoader
    { 

        public async Task<Assembly> LoadAndCompileDlls(string mainAssemblyName, string[] dllPaths, string[] csFilePaths)
        {
            try
            {
                Assembly targetAssembly = null;

                // Load existing DLLs
                foreach (var dllPath in dllPaths)
                {
                    try
                    {
                        var dll = Path.GetFullPath(dllPath);
                        Assembly assembly = Assembly.LoadFile(dll);
                        if (Path.GetFileNameWithoutExtension(dll) == mainAssemblyName || Path.GetFileName(dll) == mainAssemblyName)
                        {
                            targetAssembly = assembly;
                            Logger.Log($"Found host assembly: {dll}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error loading {dllPath}: {ex.Message}");
                        // Handle the error, e.g., decide whether to continue or abort loading.
                    }
                }

                List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();

                // Compile C# source files and accumulate syntax trees
                foreach (var csFilePath in csFilePaths)
                {
                    try
                    {
                        string code = File.ReadAllText(csFilePath);
                        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
                        syntaxTrees.Add(syntaxTree);

                        Logger.Log($"Added syntax tree from {csFilePath}.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error reading {csFilePath}: {ex.Message}");
                        // Handle the error, e.g., decide whether to continue or abort loading.
                    }
                }


                // Create a single assembly from accumulated syntax trees
    
                string assemblyName = Path.GetRandomFileName();
                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees,
                    new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


                /*
                foreach(var csFilePath in csFilePaths)
                {
                    string csFile = File.ReadAllText(csFilePath);
                    var parser = new ClassParser(csFile);
                    var members = parser.GetMembers();
                    foreach (var m in members)
                        Console.WriteLine(m);
                        

                }



                foreach (var syntaxTree in syntaxTrees)
                {
                    var root = await syntaxTree.GetRootAsync();
                    var luminosityClasses = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .Where(classSyntax =>
                            classSyntax.BaseList?.Types.Any(type => type.ToString() == "LuminosityBehaviour") == true
                        );

                    SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

                    foreach (var classSyntax in luminosityClasses)
                    {
                        var dataField = new DataField();

                        // Mirror class fields in the data field
                        var fieldDeclarations = classSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>();
                        foreach (var fieldDeclaration in fieldDeclarations)
                        {
                            var variableName = fieldDeclaration.Declaration.Variables.FirstOrDefault()?.Identifier.Text;
                            if (!string.IsNullOrEmpty(variableName))
                            {
                                // You need to obtain the value for the variable here
                                // You might need to use the SemanticModel to get the value
                                // Let's assume you have a value for the variable named "variableValue"
                                dataField.Set(variableName, variableValue);
                            }
                        }
                    }
                }

                */

                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        foreach (var diagnostic in result.Diagnostics)
                        {
                            Logger.Log(diagnostic.ToString());
                        }
                        // Handle compilation errors.
                    }
                    else
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        Assembly compiledAssembly = Assembly.Load(ms.ToArray());
                        targetAssembly = compiledAssembly;
                        Logger.Log($"Compiled and loaded assembly.");



                    }
                }
                RefreshSeriTypes(targetAssembly);
                return targetAssembly;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while loading and compiling assemblies: {ex.Message}");
                // Handle the error at a higher level, e.g., application-level exception handling.
            }

            return null;
        }

        public static void RefreshSeriTypes(Assembly assembly)
        {
            // Get all types in the assembly
            Type[] types = assembly.GetTypes();

            // Filter types that derive from LuminosityBehaviour
            IEnumerable<Type> luminosityBehaviourTypes = types
                .Where(type => type.IsSubclassOf(typeof(LuminosityBehaviour)));



            foreach (Type type in luminosityBehaviourTypes)
            {
                Logger.Log(type.FullName);
                Engine.GetSerializer().GetConfig().KnownTypes.Add(type);
            }
        }

    }

    public class MemberCollector : CSharpSyntaxWalker
    {
        public List<FieldDeclarationSyntax> Fields { get; } = new List<FieldDeclarationSyntax>();
        public List<PropertyDeclarationSyntax> Properties { get; } = new List<PropertyDeclarationSyntax>();
        public List<LocalDeclarationStatementSyntax> Variables { get; } = new List<LocalDeclarationStatementSyntax>();

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node) => this.Fields.Add(node);
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node) => this.Properties.Add(node);
        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) => this.Variables.Add(node);
    }

    public class ClassParser
    {
        public string Code { get; set; }
        public SyntaxNode Root { get; set; }

        public ClassParser(string code)
        {
            this.Code = code;
            var tree = CSharpSyntaxTree.ParseText(code);
            this.Root = tree.GetCompilationUnitRoot();
        }

    }
}

