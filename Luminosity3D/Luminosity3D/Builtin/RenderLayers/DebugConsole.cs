
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using ImGuizmoNET;
using Luminosity3DScening;

namespace Luminosity3D.Builtin.RenderLayers
{


    public class DebugConsole : IMGUIRenderLayer
    {
        public CommandManager CommandManager { get; set; } = new CommandManager();
        private byte[] inputBuffer = new byte[256];
        private bool scrollToBottom = false;
        private bool ConsoleOpen = false;



        bool showThemeing = false;
        bool showProfiler = false;
        bool showDemos = false;
        bool inputacepted = false;

        string inputString = "";


        public DebugConsole(Renderer renderer) : base(renderer)
        {
            Logger.Log("Internal console ready, loading commands..");
            LoadBuiltinCommands();
            Settings.RegisterSetting(new TestSetting());
            Settings.RegisterSetting(new TestSetting2());
            Settings.RegisterSetting(new TimeSetting());
            IMGUIStyles.SetupVisualStudioImGuiStyle();
        }

        public override void Render()
        {
            if (InputManager.GetKeyPressed(Keys.RightShift))
            {
                ConsoleOpen = !ConsoleOpen;
            }

            if (showDemos)
            {
                ImGui.ShowDemoWindow();
            }

            if (ConsoleOpen)
            {
  
                ShowDebugConsole();
                ShowEntityViewer();
            }

            if (inputacepted)
            {
                new GameObject(inputString);
                inputacepted = false;
            }

            if (showSaveWindow)
            {
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, ImGui.GetIO().DisplaySize.Y - 200), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X, 200), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Save scene", ImGuiWindowFlags.DockNodeHost ))
                {
                    ImGui.InputText("Path", ref sceneExportPath, 256);

                    if(ImGui.Button("Save to file") && sceneExportPath != string.Empty)
                    {
                        SceneManager.ActiveScene.SerializeToFile(sceneExportPath);
                    }
                  
                }
                    
            }

        }

        string sceneExportPath = string.Empty;

        bool showSaveWindow = false;
        Component comp = null;
        private void ShowDebugConsole()
        {

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, ImGui.GetIO().DisplaySize.Y - 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X, 200), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Console", ImGuiWindowFlags.DockNodeHost | ImGuiWindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Scene"))
                    {
                        if (ImGui.MenuItem("Save"))
                        {
                            showSaveWindow = !showSaveWindow;
                            
                        }
                        ImGui.EndMenu();
                    }

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
                        if (ImGui.MenuItem("Load Internal Profiler"))
                        {
                            Logger.Log("Loading Internal Profiler from lupk..");
                            Engine.PackageLoader.LoadPackage("Profilingmod");
                        }
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Themeing"))
                    {
                        if (ImGui.MenuItem("Show", showThemeing))
                        {
                            ImGui.ShowStyleEditor();

                        }
                        if (ImGui.MenuItem("Standart Theme"))
                        {
                            IMGUIStyles.SetupImGuiStyle();
                        }
                        if (ImGui.MenuItem("Visual Studio Theme"))
                        {
                            IMGUIStyles.SetupVisualStudioImGuiStyle();
                        }
                        if (ImGui.MenuItem("OG Steam Theme"))
                        {
                            IMGUIStyles.SetupSteamImGuiStyle();
                        }
                        if (ImGui.MenuItem("Comfy Theme"))
                        {
                            IMGUIStyles.SetupComfyImGuiStyle();
                        }
                        if (ImGui.MenuItem("Dark Theme"))
                        {
                            IMGUIStyles.SetupDarkStyle();
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Demos"))
                    {

                        if (ImGui.MenuItem("IMGUI Demo"))
                        {
                            showDemos = !showDemos;
                        }

                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }
                ImGui.BeginChild("ConsoleText", new System.Numerics.Vector2(0, -ImGui.GetFrameHeightWithSpacing()), true, ImGuiWindowFlags.HorizontalScrollbar);

                int logIndex = 0; // Start from the beginning of the list
                for (int i = Logger.logList.Count - 1; i >= 0; i--)
                {
                    var log = Logger.logList[logIndex];
                    ImGui.PushStyleColor(ImGuiCol.Border, GetLogColor(log.Type));

                    // Generate a unique identifier for each child window based on logIndex
                    string childWindowName = "LogWindow" + logIndex.ToString();

                    // Use a collapsing header for each log
                    bool isLogOpen = ImGui.CollapsingHeader($"ID:[{logIndex}] " + log.Lable + " @ " + log.TimeStamp, ImGuiTreeNodeFlags.DefaultOpen);

                    if (isLogOpen)
                    {
                        // Calculate the height required for the content in childWindowName
                        float contentHeight = ImGui.CalcTextSize(log.Content).Y + ImGui.GetTextLineHeightWithSpacing();

                        ImGui.BeginChild(childWindowName, new System.Numerics.Vector2(0, contentHeight), true);

                        ImGui.TextWrapped(log.Content);

                        ImGui.EndChild();
                    }

                    logIndex++; // Move to the next log in the original order
                    ImGui.PopStyleColor();
                }


                ImGui.EndChild();

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

                // Clear the debug messages.
                ImGui.SameLine();
                if (ImGui.Button("Clear"))
                {
                    Logger.logList.Clear();
                }
            }

            ImGui.End();
        }

        private uint GetLogColor(LogType type)
        {
            switch (type)
            {
                case LogType.Information:
                    return ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green
                case LogType.Warning:
                    return ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
                case LogType.Error:
                    return ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red
                default:
                    return ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // White (fallback)
            }
        }

        string objFilePath = "./resources/unitron/scene.gltf";
        string camName = "New Camera";
        System.Numerics.Vector3 camPosition = new System.Numerics.Vector3(0, 0, 0);


        private unsafe void ShowEntityViewer()
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X - 300, 0), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, ImGui.GetIO().DisplaySize.Y - 200), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Entity Viewer", ImGuiWindowFlags.DockNodeHost))
            {
                // Check for right-click anywhere
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {
                    // Show a popup menu for right-clicking anywhere
                    ImGui.OpenPopup("GlobalPopup");
                }

                if (ImGui.BeginPopup("GlobalPopup"))
                {
                    if (ImGui.MenuItem("Create GameObject"))
                    {
                        new GameObject();
                    }

                    if (ImGui.BeginMenu("Create 3D Entity"))
                    {
                        Create3DObject();
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Create Camera Entity"))
                    {
                        CreateCamera();
                        ImGui.EndMenu();
                    }

                    ImGui.EndPopup();
                }

                // Column 2: Tree view
                ImGui.NextColumn(); // Move to the next column

                // Group to contain the tree view and input box/buttons
                ImGui.BeginGroup();

                var ents = SceneManager.ActiveScene.Entities.OrderBy(x => x.ExecutionOrder).Reverse().ToList();
                for (int i = ents.Count() - 1; i >= 0; i--)
                {
                    var entity = ents[i];

                    DisplayEntity(entity);
                }

                // End the group containing tree view and input box/buttons
                ImGui.EndGroup();
            }

            ImGui.End();
        }

        private unsafe void DisplayEntity(GameObject entity)
        {
            if (ImGui.TreeNode("Name: " + entity.Name + " HashCode: " + entity.GetHashCode()))
            {
                if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {


                    // Right-clicked on the entity tree node, show the popup
                    ImGui.OpenPopup("EntityPopup");
                }

                if (ImGui.BeginPopup("EntityPopup"))
                {
                    ShowEntityPopup(entity);
                    ImGui.EndPopup();
                }



                foreach (var child in entity.Childs)
                {
                    DisplayEntity(child);
                }



                // Rest of your TreeNode content here
                var cam = SceneManager.ActiveScene.activeCam;
                var trans = entity.GetComponent<TransformComponent>();

                if (cam != null && trans != null)
                {
                    var floats = LMath.MatriciesToFloats(cam.ViewMatrix, cam.ProjectionMatrix, trans.GetTransformMatrix());
                    var view = floats[0];
                    var proj = floats[1];
                    var mat = floats[2];

                    ImGuizmo.SetRect(0, 0, Engine.Renderer.Size.X, Engine.Renderer.Size.Y);
                    ImGuizmo.Enable(true);
                    
                    //ImGuizmo.Manipulate(ref view, ref proj, OPERATION.SCALE, MODE.LOCAL, ref mat);
                }

                foreach (var comp in entity.components.Values)
                {
                    if (ImGui.TreeNode("Component: " + comp.GetType().ToString()))
                    {
                        if (ImGui.TreeNode("Properties"))
                        {
                            DisplayReflectionBasedExplorerNodes(comp);
                            ImGui.TreePop();
                        }

                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }

            
        }

        private void ShowEntityPopup(GameObject entity)
        {
            if (ImGui.MenuItem("Create GameObject"))
            {
                entity.Childs.Add(new GameObject(false));
            }

            if (ImGui.Button("Load .obj File"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    // You can use objFilePath to load the .obj file here
                    // Example: LoadObjFile(objFilePath);
                    entity.Childs.Add(EntitySummoner.CreatePBREntity("3DObj", objFilePath, new System.Numerics.Vector3(0, 0, 0)));
                }
            }

            if (ImGui.BeginMenu("Attach Component"))
            {
                var componentType = typeof(LuminosityBehaviour);
                var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => componentType.IsAssignableFrom(p) && p != componentType);

                foreach (var derivedType in derivedTypes)
                {
                    if (ImGui.MenuItem(derivedType.Name))
                    {
                        entity.AddComponent((LuminosityBehaviour)Activator.CreateInstance(derivedType));

                        
                    }
                }

         

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Delete Entity"))
            {
                entity.Kill();
            }

     



        }

        void DisplayClassFieldsAndProperties(object classObject)
        {
            var classMembers = classObject.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property);

            foreach (var classMember in classMembers)
            {
                if (classMember is FieldInfo classFieldInfo)
                {
                    if (classFieldInfo.FieldType == typeof(bool))
                    {
                        bool fieldValue = (bool)classFieldInfo.GetValue(classObject);
                        if (ImGui.Checkbox($"{classFieldInfo.Name}", ref fieldValue))
                        {
                            classFieldInfo.SetValue(classObject, fieldValue);
                        }
                    }
                    else if (classFieldInfo.FieldType == typeof(int))
                    {
                        int fieldValue = (int)classFieldInfo.GetValue(classObject);
                        if (ImGui.InputInt($"{classFieldInfo.Name}", ref fieldValue))
                        {
                            classFieldInfo.SetValue(classObject, fieldValue);
                        }
                    }
                    else if (classFieldInfo.FieldType == typeof(float))
                    {
                        float fieldValue = (float)classFieldInfo.GetValue(classObject);
                        if (ImGui.InputFloat($"{classFieldInfo.Name}", ref fieldValue))
                        {
                            classFieldInfo.SetValue(classObject, fieldValue);
                        }
                    }
                    else if (classFieldInfo.FieldType == typeof(string))
                    {
                        string fieldValue = (string)classFieldInfo.GetValue(classObject);
                        ImGui.Text($"{classFieldInfo.Name}");
                        ImGui.SameLine();

                        if (fieldValue == null)
                        {
                            fieldValue = string.Empty;
                        }

                        if (ImGui.InputText($"{classFieldInfo.Name}##Input", ref fieldValue, 256))
                        {
                            classFieldInfo.SetValue(classObject, fieldValue);
                        }
                    }
                    else if (classFieldInfo.FieldType == typeof(Vector3)) // Handle Vector3 fields
                    {
                        Vector3 vectorValue = (Vector3)classFieldInfo.GetValue(classObject);
                        var rawVec = new System.Numerics.Vector3(vectorValue.X, vectorValue.Y, vectorValue.Z);
                        ImGui.Text($"{classFieldInfo.Name}");
                        ImGui.SameLine();
                        if (ImGui.InputFloat3($"{classFieldInfo.Name}##Input", ref rawVec))
                        {
                            vectorValue = new Vector3(rawVec.X, rawVec.Y, rawVec.Z);
                            classFieldInfo.SetValue(classObject, vectorValue);
                        }
                    }
                    // Add more type checks for other data types as needed
                }
                else if (classMember is PropertyInfo classPropertyInfo)
                {
                    if (classPropertyInfo.PropertyType == typeof(bool))
                    {
                        bool propertyValue = (bool)classPropertyInfo.GetValue(classObject);
                        if (ImGui.Checkbox($"{classPropertyInfo.Name}", ref propertyValue))
                        {
                            classPropertyInfo.SetValue(classObject, propertyValue);
                        }
                    }
                    else if (classPropertyInfo.PropertyType == typeof(int))
                    {
                        int propertyValue = (int)classPropertyInfo.GetValue(classObject);
                        if (ImGui.InputInt($"{classPropertyInfo.Name}", ref propertyValue))
                        {
                            classPropertyInfo.SetValue(classObject, propertyValue);
                        }
                    }
                    else if (classPropertyInfo.PropertyType == typeof(float))
                    {
                        float propertyValue = (float)classPropertyInfo.GetValue(classObject);
                        if (ImGui.InputFloat($"{classPropertyInfo.Name}", ref propertyValue))
                        {
                            classPropertyInfo.SetValue(classObject, propertyValue);
                        }
                    }
                    else if (classPropertyInfo.PropertyType == typeof(string))
                    {
                        string propertyValue = (string)classPropertyInfo.GetValue(classObject);
                        ImGui.Text($"{classPropertyInfo.Name}");
                        ImGui.SameLine();

                        if (propertyValue == null)
                        {
                            propertyValue = string.Empty;
                        }

                        if (ImGui.InputText($"{classPropertyInfo.Name}##Input", ref propertyValue, 256))
                        {
                            classPropertyInfo.SetValue(classObject, propertyValue);
                        }
                    }
                    else if (classPropertyInfo.PropertyType == typeof(Vector3)) // Handle Vector3 properties
                    {
                        Vector3 vectorValue = (Vector3)classPropertyInfo.GetValue(classObject);
                        var rawVec = new System.Numerics.Vector3(vectorValue.X, vectorValue.Y, vectorValue.Z);
                        ImGui.Text($"{classPropertyInfo.Name}");
                        ImGui.SameLine();
                        if (ImGui.InputFloat3($"{classPropertyInfo.Name}##Input", ref rawVec))
                        {
                            vectorValue = new Vector3(rawVec.X, rawVec.Y, rawVec.Z);
                            classPropertyInfo.SetValue(classObject, vectorValue);
                        }
                    }
                    // Add more type checks for other data types as needed
                }
            }
        }

        void DisplayReflectionBasedExplorerNodes(LuminosityBehaviour component)
        {
            
            if (component is IImguiSerialize imguiSeri)
            {
                ImGui.Text("Serialized UI:");
                ImGui.Separator();

                imguiSeri.EditorUI();

                ImGui.Separator();
                //return;
            }

            if (ImGui.TreeNode("Serializeable Fields"))
            {
                ImGui.BeginGroup();

                var fieldsAndProperties = component.GetType()
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Method);

                foreach (var member in fieldsAndProperties)
                {
                    if (member is FieldInfo fieldInfo)
                    {
                        // Check if the field has the SerializeField attribute
                        if (fieldInfo.IsDefined(typeof(SerializeFieldAttribute), false))
                        {
                            object fieldValueObj = fieldInfo.GetValue(component);
                            // Display the field value or do something with it
                            ImGui.Text($"{fieldInfo.Name}: {fieldValueObj}");
                        }
                    }
                    else if (member is PropertyInfo propertyInfo)
                    {
                        // Check if the property has the SerializeField attribute
                        if (propertyInfo.IsDefined(typeof(SerializeFieldAttribute), false))
                        {
                            object propertyValueObj = propertyInfo.GetValue(component);
                            // Display the property value or do something with it
                            ImGui.Text($"{propertyInfo.Name}: {propertyValueObj}");
                        }
                    }
                    else if (member is MethodInfo methInfo)
                    {
                        if(methInfo.IsDefined(typeof(SerializeFieldAttribute), false))
                        {
                            // Check if the property has the SerializeField attribute
                            if (ImGui.Button(methInfo.Name)) // Display a button with the method name
                            {
                                // Invoke the method
                                methInfo.Invoke(member, null); // Replace yourObjectInstance with the actual object instance
                            }
                        }

                    }
                }

                ImGui.EndGroup();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Fields/Properties"))
            {

                // Display fields and properties of the component using reflection

                ImGui.BeginGroup();

                var fieldsAndProperties = component.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
                  .Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property);

                foreach (var member in fieldsAndProperties)
                {
                    if (member is FieldInfo fieldInfo)
                    {
                        object fieldValueObj = fieldInfo.GetValue(component);
                        if (fieldValueObj != null && fieldValueObj.GetType().IsClass)
                        {
                            // Display the class name and its properties
                            ImGui.Text($"Instance Name: {fieldInfo.Name}, Type: {fieldInfo.GetType()}");
                            ImGui.SameLine();
                            if (ImGui.TreeNode($"##{fieldInfo.Name}Node"))
                            {
                                DisplayClassFieldsAndProperties(fieldValueObj);
                                ImGui.TreePop();
                            }
                        }
                        else
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
                                if (ImGui.InputFloat3($"{fieldInfo.Name}##Input", ref rawVec)) ;
                                {
                                    vectorValue = new Vector3(rawVec.X, rawVec.Y, rawVec.Z);
                                    fieldInfo.SetValue(component, vectorValue);
                                }
                            }
                            // Add more type checks for other data types as needed
                        }
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

        private void CreateCamera()
        {
            // Input field for entering .obj file path
            ImGui.InputText("Camera Name", ref camName, 256); // Adjust the buffer size as needed
            ImGui.InputFloat3("Cam Position", ref camPosition);

            if (ImGui.Button("Summon"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    // You can use objFilePath to load the .obj file here
                    // Example: LoadObjFile(objFilePath);
                    EntitySummoner.CreateCamera(camName, camPosition, true);
                }
            }

        }

        private void Create3DObject()
        {
            // Input field for entering .obj file path
            ImGui.InputText("##ObjFilePath", ref objFilePath, 256); // Adjust the buffer size as needed



            if (ImGui.Button("Load .obj File With FPS Controller"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    // You can use objFilePath to load the .obj file here
                    // Example: LoadObjFile(objFilePath);
                    EntitySummoner.CreateFPSController("3DObj", objFilePath, System.Numerics.Vector3.Zero);
                }
            }

            if (ImGui.Button("Load .obj File"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    // You can use objFilePath to load the .obj file here
                    // Example: LoadObjFile(objFilePath);
                    EntitySummoner.CreatePBREntity("3DObj", objFilePath, new System.Numerics.Vector3(0, 0, 0));
                }
            }

            if (ImGui.Button("Load .obj File With Sine Mover"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    // You can use objFilePath to load the .obj file here
                    // Example: LoadObjFile(objFilePath);
                    EntitySummoner.CreatePBREntity("3DObj", objFilePath, System.Numerics.Vector3.Zero);
               
                }
            }

            if (ImGui.Button("Load .obj File With RigidBody Physics and ConvexHull Collider"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
  
                    EntitySummoner.CreatePBREntityWithRbConvexHull("3DObj", objFilePath, System.Numerics.Vector3.Zero);
                }
            }

            if (ImGui.Button("Load .obj File With RigidBody Physics Static"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    EntitySummoner.CreatePBREntityWithRbStatic("3DObj", objFilePath, System.Numerics.Vector3.Zero);
                }
            }


            if (ImGui.Button("Load .obj File With RigidBody Physics"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath))
                {
                    EntitySummoner.CreatePBREntityWithRb("3DObj", objFilePath, System.Numerics.Vector3.Zero);
                }
            }

            if (ImGui.Button("Load .obj File With RigidBody Physics and Sine Mover"))
            {
                // Handle loading .obj file using objFilePath
                if (!string.IsNullOrWhiteSpace(objFilePath) && File.Exists(objFilePath))
                {
                    EntitySummoner.CreatePBREntityWithRbAndSine("3DObj", objFilePath, System.Numerics.Vector3.Zero);
                    
                }
            }
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
            CommandManager.RegisterCommand(new NoesisCommand());
            CommandManager.RegisterCommand(new JoinCommand());
            CommandManager.RegisterCommand(new MakeServerCommand());
            CommandManager.RegisterCommand(new SayCommand());
            CommandManager.RegisterCommand(new SaveCommand());
            CommandManager.RegisterCommand(new LoadCommand());
            
            Logger.Log($"Loaded {CommandManager.Commands.Count()} commands into the manager!");
        }

    }


    #region Command System
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
            string trimmedCommandLine = commandLine.Trim();
            string[] commandParts = trimmedCommandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length == 0)
            {
                Logger.Log("Empty command. Try 'help' for a list of commands.");
                return;
            }

            string commandName = commandParts[0];
            string[] args = commandParts.Skip(1).ToArray();

            DebugCommand command = Commands.FirstOrDefault(x => x.Command == commandName);

            if (command != null)
            {
                command.Execute(args);
            }
            else
            {
                Logger.Log($"The Command: {commandName} was not found. Try 'help' for a list of commands.");
            }
        }








        public void RegisterCommand(DebugCommand command)
        {
            Commands.Add(command);
        }
    }

    public class TestNoesis : NoesisUI
    {
        public TestNoesis(string xml) : base(xml)
        {
            
        }

        
    }


    public class SayCommand : DebugCommand
    {
        public override string Command { get => "say"; }
        public override string Description { get => "write a networked message as a raw string, could be used for custom protocols"; }

        public override void Execute(string[] args)
        {
            Net.SendMessageToAllClients(args[0]);


        }
    }

    public class JoinCommand : DebugCommand
    {
        public override string Command { get => "join"; }
        public override string Description { get => "Join a server"; }

        public override void Execute(string[] args)
        {
            Net.JoinServer(args[0], 42069);


        }
    }

    public class MakeServerCommand : DebugCommand
    {
        public override string Command { get => "createserver"; }
        public override string Description { get => "Create a server"; }

        public override void Execute(string[] args)
        {
            Net.StartServer(args[0], 42069);


        }
    }

    public class SaveCommand : DebugCommand
    {
        public override string Command { get => "savescene"; }
        public override string Description { get => "Noesis UI Demo"; }

        public override void Execute(string[] args)
        {
            var scenePath = $"./scenes/{args[0]}";

            if (!Directory.Exists(scenePath))
            {
                Directory.CreateDirectory(scenePath);
            }

            foreach(var go in SceneManager.ActiveScene.Entities)
            {
                GameObjectSerializer.SerializeToPath(go, scenePath);
            }


        }
    }

    public class LoadCommand : DebugCommand
    {
        public override string Command { get => "loadscene"; }
        public override string Description { get => "Noesis UI Demo"; }

        public override void Execute(string[] args)
        {


            
            var scenePath = $"./scenes/{args[0]}";



            if (!Directory.Exists(scenePath))
            {
                Logger.Log("No scene found..");
            }

            SceneManager.LoadScene(args[0]);
            

            


        }
    }





    public class NoesisCommand : DebugCommand
    {
        public override string Command { get => "noesisdemo"; }
        public override string Description { get => "Noesis UI Demo"; }

        public override void Execute(string[] args)
        {
            Engine.Renderer.AddLayer(new TestNoesis(File.ReadAllText("./resources/noesisui.xml")));



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
                    foreach (var line in File.ReadLines(configPath))
                    {
                        Engine.Console.CommandManager.RunCommand(line);
                    }
                    timer.Stop();
                    Logger.Log($"Config/Script loaded in: {timer.ElapsedMilliseconds}ms!");
                }
                else
                {
                    Logger.Log($"Couldn't find the config/script: {args[0].ToString()}", LogType.Error);
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
            if (args.Length > 0)
            {
                var pakName = args[0];
                var packageLoader = Engine.PackageLoader;
                if (packageLoader.PakExist(pakName))
                {
                    packageLoader.LoadPackage(pakName);
                }
                else
                {
                    Logger.Log($"Couldn't find the lupk: {pakName}.lupk!", LogType.Warning);
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
            Logger.Log("Enumerating registered commands..", LogType.Debug);
            foreach (var command in Engine.Console.CommandManager.Commands)
            {
                Logger.Log("-------------------------------------------------------------------------------------", LogType.Debug);
                Logger.Log("Commmand: " + command.Command, LogType.Debug);
                Logger.Log("Description: " + command.Description, LogType.Debug);
            }
            Logger.Log("-------------------------------------------------------------------------------------", LogType.Debug);
            Logger.Log($"Jupe mods help, enumerated: {Engine.Console.CommandManager.Commands.Count()} commands!", LogType.Debug);
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
                new GameObject(args[0]);
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
                    Engine.Renderer.WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen;
                    Logger.Log("Window state set to fullscreen!");
                }
                else if (res == false)
                {
                    Engine.Renderer.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
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
            Engine.Renderer.Close();
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
    #endregion
}
