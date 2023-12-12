using Luminosity3D.Utils;
using Luminosity3D;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using Luminosity3D.Builtin.RenderLayers;
using ImGuiNET;
using OpenTK.Windowing.Common.Input;
using BulletSharp;
using Assimp;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using Luminosity3D.Rendering;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Camera = Luminosity3D.Builtin.Camera;
using System.Runtime.InteropServices;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using Face = Assimp.Face;
using Luminosity3D.Builtin;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;
using BulletSharp.Math;
using Vector3 = OpenTK.Mathematics.Vector3;
using Luminosity3D.EntityComponentSystem;
using ImGuizmoNET;
using Noesis;
using Matrix4 = OpenTK.Mathematics.Matrix4;
using Path = System.IO.Path;
using Matrix = BulletSharp.Math.Matrix;
using Marshal = System.Runtime.InteropServices.Marshal;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using Luminosity3D.PKGLoader;
using Luminosity3DScening;
using System.Reflection;
using Luminosity3D.Rendering.Bloom;
using MyGame;
using System.Windows;
using Luminosity3D.jfmod;

namespace Luminosity3DRendering
{


    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        //private Texture _fontTexture;

        private int _fontTexture;

        private int _shader;
        private int _shaderFontTextureLocation;
        private int _shaderProjectionMatrixLocation;

        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        private static bool KHRDebugAvailable = false;

        private int GLVersion;
        private bool CompatibilityProfile;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            GLVersion = major * 100 + minor * 10;

            KHRDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

            CompatibilityProfile = (GL.GetInteger((GetPName)All.ContextProfileMask) & (int)All.ContextCompatibilityProfileBit) != 0;

            IntPtr context = ImGui.CreateContext();
            //ImGuizmo.SetImGuiContext(context);
            ImGuizmo.SetImGuiContext(context);
            
            ImGui.SetCurrentContext(context);

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            //ImGuizmo.BeginFrame();
            ImGui.NewFrame();
            //ImGuizmo.BeginFrame();
            ImGuizmo.BeginFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);
            LabelObject(ObjectLabelIdentifier.VertexArray, _vertexArray, "ImGui");

            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

            _shader = CreateProgram("ImGui", VertexSource, FragmentSource);
            _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
            _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(prevVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

            CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
            LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            // Restore state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);

            io.Fonts.SetTexID((IntPtr)_fontTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
  
                RenderImDrawData(ImGui.GetDrawData());
          
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGui.NewFrame();
            
           
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = wnd.MouseState;
            KeyboardState KeyboardState = wnd.KeyboardState;

            io.MouseDown[0] = MouseState[MouseButton.Left];
            io.MouseDown[1] = MouseState[MouseButton.Right];
            io.MouseDown[2] = MouseState[MouseButton.Middle];
            io.MouseDown[3] = MouseState[MouseButton.Button4];
            io.MouseDown[4] = MouseState[MouseButton.Button5];

            var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            // Get intial state.
            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
            Span<int> prevScissorBox = stackalloc int[4];
            unsafe
            {
                fixed (int* iptr = &prevScissorBox[0])
                {
                    GL.GetInteger(GetPName.ScissorBox, iptr);
                }
            }
            Span<int> prevPolygonMode = stackalloc int[2];
            unsafe
            {
                fixed (int* iptr = &prevPolygonMode[0])
                {
                    GL.GetInteger(GetPName.PolygonMode, iptr);
                }
            }

            if (GLVersion <= 310 || CompatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(_vertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            GL.UseProgram(_shader);
            GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
            GL.Uniform1(_shaderFontTextureLocation, 0);
            CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            CheckGLError("VAO");

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                CheckGLError($"Data Vert {n}");

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                CheckGLError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        CheckGLError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            // Reset state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVAO);
            GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
            GL.BlendFuncSeparate(
                (BlendingFactorSrc)prevBlendFuncSrcRgb,
                (BlendingFactorDest)prevBlendFuncDstRgb,
                (BlendingFactorSrc)prevBlendFuncSrcAlpha,
                (BlendingFactorDest)prevBlendFuncDstAlpha);
            if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
            if (GLVersion <= 310 || CompatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, (PolygonMode)prevPolygonMode[0]);
                GL.PolygonMode(MaterialFace.Back, (PolygonMode)prevPolygonMode[1]);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vertexArray);
            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);

            GL.DeleteTexture(_fontTexture);
            GL.DeleteProgram(_shader);
        }

        public static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
        {
            if (KHRDebugAvailable)
                GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
        }

        static bool IsExtensionSupported(string name)
        {
            int n = GL.GetInteger(GetPName.NumExtensions);
            for (int i = 0; i < n; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension == name) return true;
            }

            return false;
        }

        public static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
        {
            int program = GL.CreateProgram();
            LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

            int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
            }

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        private static int CompileShader(string name, ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
            }

            return shader;
        }

        public static void CheckGLError(string title)
        {
            ErrorCode error;
            int i = 1;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                Debug.Print($"{title} ({i++}): {error}");
            }
        }
    }

    public abstract class NoesisUI : IRenderLayer
    {
        public Renderer.RenderLayerType LayerType => Renderer.RenderLayerType.GLRender; //just use gl layer for "other stuff"

        public Noesis.View view = null;

        private string xml = string.Empty;


        public NoesisUI(string xml)
        {
            this.xml = xml;
            InitView();
        }

        public Noesis.View GetView()
        {
            return view;
        }

        private void InitView()
        {
            Noesis.Log.SetLogCallback((level, channel, message) =>
            {
                if (channel == "")
                {
                    // [TRACE] [DEBUG] [INFO] [WARNING] [ERROR]
                    string[] prefixes = new string[] { "T", "D", "I", "W", "E" };
                    string prefix = (int)level < prefixes.Length ? prefixes[(int)level] : " ";
                    Logger.Log("[NOESIS/" + prefix + "] " + message);
                }
            });

            Noesis.GUI.SetLicense("JupesMod", "zRS01y1YtNeiVPBdnxxcJT2NEhJU4fzspf2DItbz0iURquRG");

            Noesis.GUI.Init();

            Noesis.Grid xaml = (Noesis.Grid)Noesis.GUI.ParseXaml(xml);

            // View creation to render and interact with the user interface
            // We transfer the ownership to a global pointer instead of a Ptr<> because there is no way
            // in GLUT to do shutdown and we don't want the Ptr<> to be released at global time
            view = Noesis.GUI.CreateView(xaml);
            //view.SetIs

            // Renderer initialization with an OpenGL device
            view.Renderer.Init(new Noesis.RenderDeviceGL());
        }

        public void Render()
        {
            view.SetSize(Engine.Renderer.Size.X, Engine.Renderer.Size.Y);
            view.Update(Time.time);
            view.Renderer.UpdateRenderTree();
            view.Renderer.RenderOffscreen();
            view.Renderer.Render();
        }
    }


    public class Renderer : GameWindow
    {
        public CubeMap cubeMap;
        private ViewPort crossHair;
        private Bloom bloom;

        public ImGuiController? IMGUIController = null;

        public List<IRenderLayer> renderLayers = new List<IRenderLayer>();
        public DebugConsole Console;

        public Renderer(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
            IMGUIController = new ImGuiController(Size.X, Size.Y);
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            CenterWindow(new Vector2i(1280, 720));
        }



        public enum RenderLayerType
        {
            ImGui,
            GLRender,
            Entity,
            HTML // Add more types as needed
        }


        



        //I really do be loading a texture now though!
        public static WindowIcon CreateWindowIcon()
        {
            var imagePath = "./icons/jmodicon.png"; // Specify the correct image path

            // Load the image using StbImageSharp
            using (var stream = File.OpenRead(imagePath))
            {
                var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                var windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(result.Width, result.Height, result.Data));

                return windowIcon;
            }
        }

        protected override void OnUnload()
        {
            if (SceneManager.ActiveScene.cache.caches.ContainsKey(typeof(MeshBatch)))
            {
                foreach (var model in SceneManager.ActiveScene.cache.caches[typeof(MeshBatch)])
                {
                    var batch = (MeshBatch)model;
                    batch.GetModel().Dispose();
                }
            }

        }


        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.DepthTest);
            // GL.DepthFunc(DepthFunction.Less);
            // GL.DepthFunc(DepthFunction.Lequal);

            // GL.Enable(EnableCap.CullFace);

            GL.Enable(EnableCap.FramebufferSrgb);

            GL.Enable(EnableCap.Multisample);

            GL.Enable(EnableCap.TextureCubeMapSeamless);

            GL.Enable(EnableCap.ProgramPointSize);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ClearColor(Color4.Black);

            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.ColorSum);
            Icon = CreateWindowIcon();
            var timer = new Stopwatch();
            timer.Start();

            Console = new DebugConsole(this);
            AddLayer(Console);

            Logger.Log("Starting Jupe's Mod..");
            Logger.Log("Deleting loaded lupk files..");
            PackageLoader.UnloadPaks();

            Logger.Log("Loading lupks..");

            Engine.PackageLoader.LoadPaks();
            timer.Stop();

            crossHair = new ViewPort("./resources/img/crosshair.png");
            cubeMap = new CubeMap("./resources/Cubemap/industrial_sunset_puresky_4k.hdr", CubeMapType.Type0);
            bloom = new Bloom();
            Physics.MakePlane();

            RuntimeManager.Init();
            Logger.Log("Loading Resources");
            ResourcesManager.RegisterResourceFromPath("game", "./resources/");
            Logger.Log("Resources loaded!");
            //RoslynCodeLoader.RefreshSeriTypes(Assembly.GetExecutingAssembly());
            Logger.Log($"Jupe's Mod Loaded in {timer.ElapsedMilliseconds / 1000}sec, press any key to exit..");

            //Logger.Log("Loading scene..");
            //SceneManager.LoadScene("Demo Scene");
            //Logger.Log("Scene loaded!");
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            IMGUIController.WindowResized(ClientSize.X, ClientSize.Y);
        }

        bool isgrabbed = false;



        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            bloom.ResizedFrame();
            RuntimeManager.Update();
            SceneManager.ActiveScene.NetMerge();
            IMGUIController.Update(this, (float)e.Time);
            // Calculate Delta Time (time between frames).
            float deltaTime = (float)e.Time;
            Time.deltaTime = deltaTime;
            Time.time += deltaTime * Time.timeScale;

            Physics.Step();

            SceneManager.ActiveScene.cache.PhysicsPass();

            SceneManager.ActiveScene.cache.UpdatePass();




        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);



            if (SceneManager.ActiveScene.activeCam != null && SceneManager.ActiveScene.activeCam.GetComponent<Camera>() != null)
            {
                bloom.BindBloom();

                cubeMap.RenderFrame();

                SceneManager.ActiveScene.cache.RenderPass(cubeMap);

                bloom.RenderFrame();

                crossHair.RenderFrame(Vector2.Zero, 0.03f);
               
            }

            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

            if (renderLayers.Count != 0)
            {
                for (int i = renderLayers.Count() - 1; i >= 0; i--)
                {
                    var renderLayer = renderLayers[i];
                    renderLayer.Render();
                }
            }
            IMGUIController.Render();






            SwapBuffers();
            Net.SendSceneToClients();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            IMGUIController.PressChar(e.AsString[0]);
           
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            var mouseState = InputManager.GetMouse();
            if (renderLayers.Count != 0)
            {
                for (int i = renderLayers.Count() - 1; i >= 0; i--)
                {
                    
                    var renderLayer = renderLayers[i];
                    if(renderLayer is NoesisUI ngui)
                    {
                     
                        
                        ngui.GetView().MouseMove((int)mouseState.Delta.X, (int)mouseState.Delta.Y);
                    }

           
                }
            }
        }

        protected override void OnMouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
        {
            var mouseState = InputManager.GetMouse();

            for (int i = renderLayers.Count() - 1; i >= 0; i--)
            {

                var renderLayer = renderLayers[i];
                if (renderLayer is NoesisUI ngui)
                {
                    if (e.Button == MouseButton.Left)
                    {
                        ngui.GetView().MouseButtonDown((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Left);

                    }
                    if (e.Button == MouseButton.Right)
                    {
                        ngui.GetView().MouseButtonDown((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Right);

                    }
                }
            }
        }

        protected override void OnMouseUp(OpenTK.Windowing.Common.MouseButtonEventArgs e)
        {
            var mouseState = InputManager.GetMouse();

            for (int i = renderLayers.Count() - 1; i >= 0; i--)
            {

                var renderLayer = renderLayers[i];
                if (renderLayer is NoesisUI ngui)
                {
                    if (e.Button == MouseButton.Left)
                    {
                        ngui.GetView().MouseButtonUp((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Left);

                    }
                    if (e.Button == MouseButton.Right)
                    {
                        ngui.GetView().MouseButtonUp((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Right);

                    }
                }
            }
        }

        public override void Close()
        {
            base.Close();
            Net.StopServer();
            Environment.Exit(0);
        }



        public void AddLayer(IRenderLayer layer)
        {
            renderLayers.Add(layer);
        }
    }
}
