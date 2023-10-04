using Assimp;
using Luminosity3D.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection;
using Material = Assimp.Material;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using Luminosity3DRendering;

namespace Luminosity3D.Rendering
{
    public static class Textures
    {
        public static Dictionary<string, int> texts = new Dictionary<string, int>();

        public static void Cache(string key, int value, bool force = false)
        {
            if (!texts.ContainsKey(key))
            {
                texts[key] = value;
            }
            else if(force)
            {
                texts[key] = value;
            }

        }

        public static int Get(string key)
        {
            return texts[key];
        }
    }

    public class Shader
    {
        public int ProgramId { get; private set; }

        private int m_texId = -1;

        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            ProgramId = LoadShaderProgram(vertexShaderPath, fragmentShaderPath);
        }

        public static Shader BuildFromMaterialPBR(Assimp.Material material)
        {
            var shader = new Shader("./shaders/builtin/pbr.vert", "./shaders/builtin/pbr.frag");
            shader.ApplyAssimpMaterial(material);
            return shader;
        }

        public static Shader BuildFromMaterial(Assimp.Material material, string vert, string frag)
        {
            var shader = new Shader(vert,frag);
            shader.ApplyAssimpMaterial(material);
            return shader;
        }

        private void LoadTexture(String fileName)
        {
            

            fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
            if (!File.Exists(fileName))
            {
                return;
            }
            Bitmap textureBitmap = new Bitmap(fileName);
            BitmapData TextureData =
                    textureBitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb
                );
            m_texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_texId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, textureBitmap.Width, textureBitmap.Height, 0,
                PixelFormat.Bgr, PixelType.UnsignedByte, TextureData.Scan0);
            textureBitmap.UnlockBits(TextureData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void ApplyAssimpMaterial(Assimp.Material mat)
        {
            if (mat.GetMaterialTextureCount(TextureType.Diffuse) > 0)
            {
                TextureSlot tex;
                if (mat.GetMaterialTexture(TextureType.Diffuse, 0, out tex))
                    LoadTexture(tex.FilePath);
            }

            Vector4 diffuseColor = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            if (mat.HasColorDiffuse)
            {
                diffuseColor = LGLE.ToVector4(mat.ColorDiffuse);
            }

            Vector4 specularColor = new Vector4(0, 0, 0, 1.0f);
            if (mat.HasColorSpecular)
            {
                specularColor = LGLE.ToVector4(mat.ColorSpecular);
            }

            Vector4 ambientColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
            if (mat.HasColorAmbient)
            {
                ambientColor = LGLE.ToVector4(mat.ColorAmbient);
            }

            Vector4 emissiveColor = new Vector4(0, 0, 0, 1.0f);
            if (mat.HasColorEmissive)
            {
                emissiveColor = LGLE.ToVector4(mat.ColorEmissive);
            }

            float shininess = 1;
            float shininessStrength = 1;
            if (mat.HasShininess)
            {
                shininess = mat.Shininess;
            }
            if (mat.HasShininessStrength)
            {
                shininessStrength = mat.ShininessStrength;
            }

            SetUniform("material.diffuse", diffuseColor);
            SetUniform("material.specular", specularColor);
            SetUniform("material.ambient", ambientColor);
            SetUniform("material.emission", emissiveColor);
            SetUniform("material.shininess", shininess);
            SetUniform("material.shininessStrength", shininessStrength);
        }

        private int LoadShaderProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            int vertexShader, fragmentShader, shaderProgram;

            // Compile vertex shader
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(vertexShaderPath));
            GL.CompileShader(vertexShader);

            // Compile fragment shader
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragmentShaderPath));
            GL.CompileShader(fragmentShader);

            // Create shader program and link shaders
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            
            // Delete the shaders as they're linked into our program now and no longer necessary
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }


        public void Use()
        {
            GL.UseProgram(ProgramId);
        }

        public void SetUniform(string name, int value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform1(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, float value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform1(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, Vector3 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform3(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, Vector4 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform4(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, Matrix4 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.UniformMatrix4(location, false, ref value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void BindTexture(string name, int textureUnit, int textureId)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.Uniform1(location, textureUnit);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void EnableAttribute(string name)
        {
            int location = GL.GetAttribLocation(ProgramId, name);
            if (location != -1)
            {
                GL.EnableVertexAttribArray(location);
            }
            else
            {
                // Handle attribute not found error
            }
        }

        public void DisableAttribute(string name)
        {
            int location = GL.GetAttribLocation(ProgramId, name);
            if (location != -1)
            {
                GL.DisableVertexAttribArray(location);
            }
            else
            {
                // Handle attribute not found error
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }
    }
}
