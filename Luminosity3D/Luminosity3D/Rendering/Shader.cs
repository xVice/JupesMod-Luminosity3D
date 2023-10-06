using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using Luminosity3D.Utils;

namespace Luminosity3D.Rendering
{
    public class TextureProgram : IDisposable
    {
        private int Handle;
        private Tuple<TextureUnit, int>? Unit;
        private PixelInternalFormat PixelInternFormat = PixelInternalFormat.SrgbAlpha;
        private TextureUnit Textureunit = TextureUnit.Texture0;
        public TextureProgram(string path)
        {
            Init(path);
        }
        public TextureProgram(string path, PixelInternalFormat pixelformat)
        {
            PixelInternFormat = pixelformat;
            Init(path);
        }
        public TextureProgram(string path, TextureUnit unit)
        {
            Textureunit = unit;
            Init(path);
        }
        public TextureProgram(string path, PixelInternalFormat pixelformat, TextureUnit unit)
        {
            PixelInternFormat = pixelformat; Textureunit = unit;
            Init(path);
        }
        private void Init(string path)
        {
            if (!File.Exists(path))
                throw new Exception($"Texture file doesnt exist: {path}, (place, anything in either, the resource folder of jmod or the mod you might be making and use the Resources class!)");

            Unit = new Tuple<TextureUnit, int>(Textureunit, getUnitInt(Textureunit));

            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            StbImage.stbi_set_flip_vertically_on_load(1);
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternFormat,
                    image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        }
        public int Use { get => _Use(); }
        private int _Use()
        {

            GL.ActiveTexture(Unit!.Item1);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            return Unit.Item2;

        }
        private int getUnitInt(TextureUnit Textureunit)
        {
            string unitNum = $"{Textureunit}";
            switch (unitNum.Length)
            {
                case 8:
                    return int.Parse(unitNum[(unitNum.Length - 1)..]);

                case 9:
                    return int.Parse(unitNum[(unitNum.Length - 2)..]);

                default:
                    return 0;
            }
        }
        public void Dispose() => GL.DeleteTexture(Handle);
    }

    public class ShaderProgram : IDisposable
    {
        private readonly int Handle;
        private readonly Dictionary<string, int> uniformLocations;
        private string CurrentShader = string.Empty;
        public ShaderProgram(string vertFile, string fragFile, string geomFile = "")
        {
            CurrentShader = $"{vertFile} {fragFile} {geomFile}";

            Handle = GL.CreateProgram();

            int vertShader = CreateShader(vertFile, ShaderType.VertexShader);
            int fragShader = CreateShader(fragFile, ShaderType.FragmentShader);

            GL.AttachShader(Handle, vertShader);
            GL.AttachShader(Handle, fragShader);

            int geomShader = 0;
            if (geomFile != string.Empty)
            {
                geomShader = CreateShader(geomFile, ShaderType.GeometryShader);
                GL.AttachShader(Handle, geomShader);
            }

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                throw new Exception($"Ocorreu um erro ao vincular o programa \n{CurrentShader} \n{GL.GetShaderInfoLog(Handle)}");
            }

            // Desanexar os shaders, e depois o apague-os
            DetachDeleteShaders(vertShader);
            DetachDeleteShaders(fragShader);
            if (geomShader != 0)
            {
                DetachDeleteShaders(geomShader);
            }

            // aloque o dicionário para armazenar todos os uniforms.GL.GetUniformLocation(Handle, nome)new Dictionary<string, int>();
            uniformLocations = new Dictionary<string, int>();

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Loop through all the uniforms
            for (var i = 0; i < numberOfUniforms; i++)
            {
                string uniformName = GL.GetActiveUniform(Handle, i, out var size, out _);

                // Handle array uniforms
                if (size > 1)
                {
                    for (int j = 0; j < size; j++)
                    {
                        string arrayUniformName = $"{uniformName.Substring(0, uniformName.Length - 2)}[{j}]";
                        var location = GL.GetUniformLocation(Handle, arrayUniformName);
                        uniformLocations.Add(arrayUniformName, location);
                    }
                }
                else
                {
                    // Handle single uniforms
                    var location = GL.GetUniformLocation(Handle, uniformName);
                    uniformLocations.Add(uniformName, location);
                }
            }
        }
        private int CreateShader(string shaderCode, ShaderType type)
        {
            int shader = GL.CreateShader(type);

            if (File.Exists(shaderCode))
            {
                GL.ShaderSource(shader, File.ReadAllText(shaderCode));
            }
            else
            {
                GL.ShaderSource(shader, shaderCode);
            }

            // compila o shader
            GL.CompileShader(shader);

            // Verifica se há erros de compilação
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

            if (code != (int)All.True)
            {
                string log = $"A error occured during shader compilation of \n{CurrentShader} \n{GL.GetShaderInfoLog(shader)}";
                Logger.LogToFile(log);
                throw new Exception(log);
            }


            return shader;
        }
        private void DetachDeleteShaders(int shader)
        {
            GL.DetachShader(Handle, shader);
            GL.DeleteShader(shader);
        }
        public void Use()
        => GL.UseProgram(Handle);

        public void Dispose()
        => GL.DeleteProgram(Handle);

        public int GetAttribLocation(string attribName)
        => GL.GetAttribLocation(Handle, attribName);

        public void SetUniform(string nome, bool dados)
        => GL.Uniform1(uniformLocations[nome], (dados ? 1 : 0));

        public void SetUniform(string nome, int dados)
        => GL.Uniform1(uniformLocations[nome], dados);

        public void SetUniform(string nome, float dados)
        => GL.Uniform1(uniformLocations[nome], dados);

        public void SetUniform(string nome, Vector2 dados)
        => GL.Uniform2(uniformLocations[nome], dados);

        public void SetUniform(string nome, Vector3 dados)
        => GL.Uniform3(uniformLocations[nome], dados);

        public void SetUniform(string nome, Vector4 dados)
        => GL.Uniform4(uniformLocations[nome], dados);

        public void SetUniform(string nome, Color4 dados)
        => GL.Uniform4(uniformLocations[nome], dados);

        public void SetUniform(string nome, Matrix4 dados)
        => GL.UniformMatrix4(uniformLocations[nome], true, ref dados);


        //---------------------------------- System Numerics Values -----------------------------------
        public void SetUniform(string nome, System.Numerics.Vector2 dados)
        => GL.Uniform2(uniformLocations[nome], new Vector2(dados.X, dados.Y));

        public void SetUniform(string nome, System.Numerics.Vector3 dados)
        => GL.Uniform3(uniformLocations[nome], new Vector3(dados.X, dados.Y, dados.Z));

        public void SetUniform(string nome, System.Numerics.Vector4 dados)
        => GL.Uniform4(uniformLocations[nome], new Vector4(dados.X, dados.Y, dados.Z, dados.W));
        public void SetUniform(string nome, System.Numerics.Matrix4x4 dados)
        {
            Matrix4 Matrix = new Matrix4(
                dados.M11, dados.M12, dados.M13, dados.M14,
                dados.M21, dados.M22, dados.M23, dados.M24,
                dados.M31, dados.M32, dados.M33, dados.M34,
                dados.M41, dados.M42, dados.M43, dados.M44);

            GL.UniformMatrix4(uniformLocations[nome], true, ref Matrix);
        }


        //---------------------------------- Bullet Math Values -----------------------------------
        public void SetUniform(string nome, BulletSharp.Math.Vector3 dados)
        => GL.Uniform3(uniformLocations[nome], new Vector3((float)dados.X, (float)dados.Y, (float)dados.Z));

        public void SetUniform(string nome, BulletSharp.Math.Vector4 dados)
        => GL.Uniform4(uniformLocations[nome], new Vector4((float)dados.X, (float)dados.Y, (float)dados.Z, (float)dados.W));
        public void SetUniform(string nome, BulletSharp.Math.Matrix dados)
        {
            Matrix4 Matrix = new Matrix4(
                (float)dados.M11, (float)dados.M12, (float)dados.M13, (float)dados.M14,
                (float)dados.M21, (float)dados.M22, (float)dados.M23, (float)dados.M24,
                (float)dados.M31, (float)dados.M32, (float)dados.M33, (float)dados.M34,
                (float)dados.M41, (float)dados.M42, (float)dados.M43, (float)dados.M44);

            GL.UniformMatrix4(uniformLocations[nome], true, ref Matrix);
        }

    }
}
