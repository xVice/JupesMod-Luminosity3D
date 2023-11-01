using Luminosity3D.Builtin;
using Luminosity3D.Rendering;
using Luminosity3DRendering;
using Luminosity3DScening;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Luminosity3D
{
    public class Outdoor
    {
        private ShaderProgram shader;
        private TextureProgram texture; 
        public Outdoor(string fileTexture)
        {
            shader = new ShaderProgram("./Samplers/Outdoor/shader.vert", "./Samplers/Outdoor/shader.frag");
            texture = new TextureProgram(fileTexture);
        }
        public Vector3 position = Vector3.Zero;
        public Vector2 scale = Vector2.Zero;
        public void RenderFrame(Matrix4 model)
        {

            shader.Use();
            shader.SetUniform("imagem", texture.Use);

            var activecam = SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
            shader.SetUniform("CameraRight", activecam.Right);
            shader.SetUniform("CameraUp", activecam.Up);
            shader.SetUniform("model", model);

            shader.SetUniform("view", activecam.ViewMatrix);
            shader.SetUniform("projection", activecam.ProjectionMatrix);

            Quad.RenderQuad();
        }
    }
}