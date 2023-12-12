using Luminosity3DScening;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MyGame
{
    public class Outdoor
    {
        private ShaderProgram shader;
        private TextureProgram texture; 
        public Outdoor(string fileTexture)
        {
            shader = new ShaderProgram("Rendering/Samplers/Outdoor/shader.vert", "Rendering/Samplers/Outdoor/shader.frag");
            texture = new TextureProgram(fileTexture);
        }
        public Vector3 position = Vector3.Zero;
        public Vector2 scale = Vector2.Zero;
        public void RenderFrame(Matrix4 model)
        {

            shader.Use();
            shader.SetUniform("imagem", texture.Use);


            shader.SetUniform("CameraRight", SceneManager.ActiveScene.activeCam.Right);
            shader.SetUniform("CameraUp", SceneManager.ActiveScene.activeCam.Up);
            shader.SetUniform("model", model);

            shader.SetUniform("view", SceneManager.ActiveScene.activeCam.ViewMatrix);
            shader.SetUniform("projection", SceneManager.ActiveScene.activeCam.ProjectionMatrix);

            Quad.RenderQuad();
        }
    }
}