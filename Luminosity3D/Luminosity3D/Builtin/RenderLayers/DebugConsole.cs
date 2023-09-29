using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.PKGLoader;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using OpenTK.ImGui;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Luminosity3D.Builtin.RenderLayers
{
    public abstract class DebugCommand
    {
        public abstract string Command { get; }
        public abstract string Description { get; }



        public abstract void Execute(string[] args);
        
    }

    public class CommandManager
    {
        public List<DebugCommand> Commands = new List<DebugCommand>();

        public CommandManager() { }

        public void RunCommand(string commandLine)
        {
            if (!commandLine.Contains(' '))
            {
                // Handle commands with no arguments here.
                DebugCommand Command = Commands.FirstOrDefault(x => x.Command == commandLine);

                if (Command != null)
                {
                    Command.Execute(new string[0]); // Pass an empty array for 0 arguments.
                }
                else
                {
                    Logger.Log($"The Command: {commandLine.Trim()} was not found, try help for getting a list of all commands!");
                }
            }
            else
            {
                string[] commandParts = commandLine.Trim().Split(' ');

                DebugCommand Command = null;

                if (commandParts.Length > 0)
                {
                    Command = Commands.FirstOrDefault(x => x.Command == commandParts[0]);
                }

                if (Command != null)
                {
                    var args = commandParts.Length > 1 ? commandParts.Skip(1).ToArray() : new string[0]; // Check if there are arguments, if not, use an empty array.
                    Command.Execute(args);
                }
                else
                {
                    Logger.Log($"The Command: {commandParts[0].Trim()} was not found, try help for getting a list of all commands!");
                }
            }
        }





        public void RegisterCommand(DebugCommand command) 
        {
            Commands.Add(command); 
        }
    }

    public class ExecCommand : DebugCommand
    {
        public override string Command { get => "exec"; }
        public override string Description { get => "Executes a file with commands and runs them in series, arg 1 should be the path of the config/script"; }

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                var configPath = args[0].ToString();
                if (File.Exists(configPath))
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    Logger.Log("Loading a config/script.");
                    foreach(var line in File.ReadLines(configPath))
                    {
                        Engine.Instance.Console.CommandManager.RunCommand(line);
                    }
                    timer.Stop();
                    Logger.Log($"Config/Script loaded in: {timer.ElapsedMilliseconds}ms!");
                }
                else
                {
                    Logger.Log($"Couldn't find the config/script: {args[0].ToString()}");
                }
                
               
            }
            else
            {
                Logger.Log("Usage: exec <path>");
            }



        }
    }

    public class LoadLUPKCommand : DebugCommand
    {
        public override string Command { get => "loadlupk"; }
        public override string Description { get => "Loads a lupk from the lupk folder, arg 1 should be the name, not the path."; }

        public override void Execute(string[] args)
        {
            if(args.Length > 0)
            {
                var pakName = args[0];
                var packageLoader = Engine.Instance.PackageLoader;
                if (packageLoader.PakExist(pakName))
                {
                    packageLoader.LoadPackage(pakName);
                }
                else
                {
                    Logger.Log($"Couldn't find the lupk: {pakName}.lupk!");
                }
            }
            else
            {
                Logger.Log("Usage: loadlupkg <name>");
            }

           
            
        }
    }

    public class HelpCommand : DebugCommand
    {
        public override string Command { get => "help"; }
        public override string Description { get => "Shows a list of all registered commands."; }

        public override void Execute(string[] args)
        {
            Logger.Log("Enumerating registered commands..");
            foreach(var command in Engine.Instance.Console.CommandManager.Commands)
            {
                Logger.Log("-------------------------------------------------------------------------------------");
                Logger.Log("Commmand: " + command.Command);
                Logger.Log("Description: " + command.Description);   
            }
            Logger.Log("-------------------------------------------------------------------------------------");
            Logger.Log($"Jupe mods help, enumerated: {Engine.Instance.Console.CommandManager.Commands.Count()} commands!");
        }
    }

    public class MeisterCommand : DebugCommand
    {
        public override string Command { get => "meister"; }
        public override string Description { get => "RandomMobamba"; }

        public override void Execute(string[] args)
        {
            Random rand = new Random(); for (int i = 0; i < 10; i++) { Logger.Log(rand.Next(0, 10).ToString()); } //One-Line Wow
        }
    }

    public class SummonCommand : DebugCommand
    {
        public override string Command { get => "summon"; }
        public override string Description { get => "Summons a entity in the active engine with arg 1 as the name."; }

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                Engine.Instance.SceneManager.ActiveScene.InstantiateEntity(new Entity(args[0]));
            }
            else
            {
                Logger.Log("Call the command like this: summon EntityName");
            }
        }
    }

    public class FullScreenCommand : DebugCommand
    {
        public override string Command { get => "fullscreen"; }
        public override string Description { get => "sets the window to fullscreen depending on the first arg which should be a bool or 1,0"; }

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                bool fullScreen = bool.TryParse(args[0], out bool res);
                if (res)
                {
                    Engine.Instance.Renderer.WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen;
                    Logger.Log("Window state set to fullscreen!");
                }
                else if(res == false)
                {
                    Engine.Instance.Renderer.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
                    Logger.Log("Window state set to normal!");
                }
                else
                {
                    Logger.Log("Use true and false, not 0 and 1. i lied in the help.");
                }
            }
            else
            {
                Logger.Log("Call the command like this: fullscreen True/False/1/0");
            }
        }
    }

    public class QuitCommand : DebugCommand
    {
        public override string Command { get => "quit"; }
        public override string Description { get => "quits the game"; }

        public override void Execute(string[] args)
        {
            Engine.Instance.Renderer.Close();
            Environment.Exit(0);
        }
    }

    public class GetActiveDirectory : DebugCommand
    {
        public override string Command { get => "getdir"; }
        public override string Description { get => "shows you what the ./ folder is"; }

        public override void Execute(string[] args)
        {
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            Logger.Log("Current directory: " + currentDirectory);
        }
    }


    public class DebugConsole : IMGUIRenderLayer
    {
        public CommandManager CommandManager { get; set; } = new CommandManager();
        private List<string> messages = new List<string>();
        private byte[] inputBuffer = new byte[256];
        private bool scrollToBottom = true;
        private bool ConsoleOpen = false;

        public DebugConsole(Renderer renderer) : base(renderer)
        {
            Logger.Log("Internal console ready, loading commands..");
            LoadBuiltinCommands();
        }

        public void LoadBuiltinCommands()
        {
            CommandManager.RegisterCommand(new HelpCommand());
            CommandManager.RegisterCommand(new SummonCommand());
            CommandManager.RegisterCommand(new FullScreenCommand());
            CommandManager.RegisterCommand(new QuitCommand());
            CommandManager.RegisterCommand(new ExecCommand());
            CommandManager.RegisterCommand(new LoadLUPKCommand());
            CommandManager.RegisterCommand(new GetActiveDirectory());
            CommandManager.RegisterCommand(new MeisterCommand());
            Logger.Log($"Loaded {CommandManager.Commands.Count()} commands into the manager!");
        }

        bool showThemeing = false;
        bool showProfiler = false;
        bool showDemos = false;

        public override void Render()
        {
            if (Engine.Instance.KeyboardState.IsKeyPressed(Keys.RightShift))
            {
                ConsoleOpen = !ConsoleOpen;
            }

            if (ConsoleOpen)
            {
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(1000, 400), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSizeConstraints(new System.Numerics.Vector2(200, 100), new System.Numerics.Vector2(float.MaxValue, float.MaxValue));

                if (ImGui.Begin("Debug Console", ref ConsoleOpen, ImGuiWindowFlags.MenuBar))
                {
                    if (ImGui.BeginMenuBar())
                    {
                        if (ImGui.BeginMenu("Window"))
                        {
                            if (ImGui.MenuItem("Close"))
                            {
                                ConsoleOpen = false;
                            }
                            ImGui.EndMenu();
                        }
 
                        if (ImGui.BeginMenu("Profiler"))
                        {
                            if (ImGui.MenuItem("Load Imgui Profiler", showProfiler))
                            {
                                Logger.Log("Loading IMGUI Profiler");
                                ImGui.ShowMetricsWindow();

                            }
                            if(ImGui.MenuItem("Load Internal Profiler"))
                            {
                                Logger.Log("Loading Internal Profiler from lupk..");
                                Engine.Instance.PackageLoader.LoadPackage("Profilingmod");
                            }
                            ImGui.EndMenu();
                        }
    
                        if (ImGui.BeginMenu("Themeing"))
                        {
                            if (ImGui.MenuItem("Show", showThemeing))
                            {
                                ImGui.ShowStyleEditor();
                                
                            }
                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Demos"))
                        {
                          
                            if (ImGui.MenuItem("IMGUI Demo"))
                            {
                                showDemos = !showDemos;
                            }

                            if (ImGui.MenuItem("Summon Demo Entitys for explorer/testing"))
                            {
                                var cament = new Entity("Test Cam");
                                cament.AddComponent<Camera>(new Camera(cament, 90, 1920 / 1080, 0.001f, 1000f));
                                cament.AddComponent<CameraController>(new CameraController(cament));
                                Engine.Instance.SceneManager.ActiveScene.InstantiateEntity(cament);


                                Engine.Instance.SceneManager.ActiveScene.InstantiateEntity(EntitySummoner.CreatePBREntity("3D Teapot", "./teapot.obj", new Vector3(0,0,0)));
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenuBar();
                    }

                    ImGui.Columns(2); // Create two columns

                    // Column 1: Display the debug messages in a scrollable text area.
                    ImGui.BeginChild("ConsoleText", new System.Numerics.Vector2(0, -ImGui.GetFrameHeightWithSpacing()), true, ImGuiWindowFlags.HorizontalScrollbar);
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0, 4));

                    foreach (var message in messages)
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f), message);
                    }

                    if (scrollToBottom)
                    {
                        ImGui.SetScrollHereY(1.0f);
                    }

                    ImGui.PopStyleVar();
                    ImGui.EndChild();

                    // Column 2: Tree view
                    ImGui.NextColumn(); // Move to the next column

                    // Group to contain the tree view and input box/buttons
                    ImGui.BeginGroup();

                    var ents = Engine.Instance.SceneManager.ActiveScene.Entities.OrderBy(x => x.ExecutionOrder).Reverse().ToList();
                    for (int i = ents.Count() - 1; i >= 0; i--)
                    {
                        var entity = ents[i];
                        if (ImGui.TreeNode(entity.Name))
                        {
                            var components = entity.Components.OrderBy(x => x.ExecutionOrder).ToList();
                            for (int j = components.Count() - 1; j >= 0; j--)
                            {
                                var component = components[j];
                                if (ImGui.TreeNode(component.GetType().ToString()))
                                {
                                    // Display fields and properties of the component using reflection
                                    ImGui.Text("Fields/Properties");
                                    ImGui.BeginGroup();

                                    var fieldsAndProperties = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
                                        .Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property);

                                    foreach (var member in fieldsAndProperties)
                                    {
                                        if (member is FieldInfo fieldInfo)
                                        {
                                            if (fieldInfo.FieldType == typeof(bool))
                                            {
                                                bool fieldValue = (bool)fieldInfo.GetValue(component);
                                                if (ImGui.Checkbox($"{fieldInfo.Name}", ref fieldValue))
                                                {
                                                    fieldInfo.SetValue(component, fieldValue);
                                                }
                                            }
                                            else if (fieldInfo.FieldType == typeof(int))
                                            {
                                                int fieldValue = (int)fieldInfo.GetValue(component);
                                                if (ImGui.InputInt($"{fieldInfo.Name}", ref fieldValue))
                                                {
                                                    fieldInfo.SetValue(component, fieldValue);
                                                }
                                            }
                                            else if (fieldInfo.FieldType == typeof(float))
                                            {
                                                float fieldValue = (float)fieldInfo.GetValue(component);
                                                if (ImGui.InputFloat($"{fieldInfo.Name}", ref fieldValue))
                                                {
                                                    fieldInfo.SetValue(component, fieldValue);
                                                }
                                            }
                                            else if (fieldInfo.FieldType == typeof(string))
                                            {
                                                string fieldValue = (string)fieldInfo.GetValue(component);
                                                ImGui.Text($"{fieldInfo.Name}");
                                                ImGui.SameLine();
                                                if (ImGui.InputText($"{fieldInfo.Name}##Input", ref fieldValue, 256)) // Adjust the buffer size as needed
                                                {
                                                    fieldInfo.SetValue(component, fieldValue);
                                                }
                                            }
                                            else if (fieldInfo.FieldType == typeof(Vector3)) // Handle Vector3 fields
                                            {
                                                Vector3 vectorValue = (Vector3)fieldInfo.GetValue(component);
                                                var rawVec = new System.Numerics.Vector3(vectorValue.X, vectorValue.Y, vectorValue.Z);
                                                ImGui.Text($"{fieldInfo.Name}");
                                                ImGui.SameLine();
                                                if (ImGui.InputFloat3($"{fieldInfo.Name}##Input", ref rawVec));
                                                {
                                                    vectorValue = new Vector3(rawVec.X, rawVec.Y, rawVec.Z);
                                                    fieldInfo.SetValue(component, vectorValue);
                                                }
                                            }
                                            // Add more type checks for other data types as needed
                                        }
                                        else if (member is PropertyInfo propertyInfo)
                                        {
                                            if (propertyInfo.PropertyType == typeof(bool))
                                            {
                                                bool propertyValue = (bool)propertyInfo.GetValue(component);
                                                if (ImGui.Checkbox($"{propertyInfo.Name}", ref propertyValue))
                                                {
                                                    propertyInfo.SetValue(component, propertyValue);
                                                }
                                            }
                                            else if (propertyInfo.PropertyType == typeof(int))
                                            {
                                                int propertyValue = (int)propertyInfo.GetValue(component);
                                                if (ImGui.InputInt($"{propertyInfo.Name}", ref propertyValue))
                                                {
                                                    propertyInfo.SetValue(component, propertyValue);
                                                }
                                            }
                                            else if (propertyInfo.PropertyType == typeof(float))
                                            {
                                                float propertyValue = (float)propertyInfo.GetValue(component);
                                                if (ImGui.InputFloat($"{propertyInfo.Name}", ref propertyValue))
                                                {
                                                    propertyInfo.SetValue(component, propertyValue);
                                                }
                                            }
                                            else if (propertyInfo.PropertyType == typeof(string))
                                            {
                                                string propertyValue = (string)propertyInfo.GetValue(component);
                                                ImGui.Text($"{propertyInfo.Name}");
                                                ImGui.SameLine();

                                                // Add a null check for propertyValue
                                                if (propertyValue == null)
                                                {
                                                    propertyValue = string.Empty; // Set it to an empty string or some default value if it's null
                                                }

                                                if (ImGui.InputText($"{propertyInfo.Name}##Input", ref propertyValue, 256)) // Adjust the buffer size as needed
                                                {
                                                    propertyInfo.SetValue(component, propertyValue);
                                                }

                                            }
                                            else if (propertyInfo.PropertyType == typeof(Vector3)) // Handle Vector3 properties
                                            {
                                                Vector3 vectorValue = (Vector3)propertyInfo.GetValue(component);
                                                var rawVec = new System.Numerics.Vector3(vectorValue.X, vectorValue.Y, vectorValue.Z);
                                                ImGui.Text($"{propertyInfo.Name}");
                                                ImGui.SameLine();
                                                if (ImGui.InputFloat3($"{propertyInfo.Name}##Input", ref rawVec))
                                                {
                                                    vectorValue = new Vector3(rawVec.X, rawVec.Y, rawVec.Z);
                                                    propertyInfo.SetValue(component, vectorValue);
                                                }
                                            }
                                            // Add more type checks for other data types as needed
                                        }
                                    }

                                    ImGui.EndGroup();

                                    ImGui.Text("Methods");
                                    ImGui.BeginGroup();
                                    // Enumerate methods and add buttons to invoke them
                                    var methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
                                    foreach (var method in methods)
                                    {
                                        if (method.GetParameters().Length == 0) // Check if the method has no parameters
                                        {
                                            if (ImGui.Button(method.Name))
                                            {
                                                method.Invoke(component, null);
                                            }
                                        }
                                    }
                                    ImGui.EndGroup();
                                    ImGui.TreePop(); // Close component node
                                }
                            }
                            ImGui.TreePop(); // Close entity node
                        }
                    }


                    ImGui.EndGroup(); // End the group containing tree view and input box/buttons

                    ImGui.Columns(1); // Reset the column layout

                    // Input field for adding new debug messages.
                    if (ImGui.InputText("##Input", inputBuffer, (uint)inputBuffer.Length, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        string enteredText = Encoding.UTF8.GetString(inputBuffer);
                        enteredText = enteredText.Trim(); // Trim the input to remove extra spaces
                        if (!string.IsNullOrEmpty(enteredText))
                            CommandManager.RunCommand(enteredText);
                    }

                    // Scroll to the end manually.
                    ImGui.SameLine();
                    if (ImGui.Button("Scroll to Bottom"))
                    {
                        scrollToBottom = true;
                    }

                    // Clear the debug messages.
                    ImGui.SameLine();
                    if (ImGui.Button("Clear"))
                    {
                        messages.Clear();
                    }
                }

                ImGui.End();
            }

            if (showDemos)
            {
                ImGui.ShowDemoWindow();
            }
        }







        public void Log(string message)
        {
            messages.Add(message);
            scrollToBottom = true;
        }
    }
}
