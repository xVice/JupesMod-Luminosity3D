
using Luminosity3DScening;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MyGame
{
    public class Tinted : IDisposable
    {
        public class Ball
        {
            public Vector3 Position;
            public Vector3 Direction;

        }
        private List<Ball> ball = new List<Ball>();
        private ShaderProgram Shader;
        public Tinted()
        {
            Shader = new ShaderProgram("Rendering/Samplers/Tinted/shader.vert", "Rendering/Samplers/Tinted/shader.frag");
        }
        public void RenderFrame()
        {
            Shader.Use();
            
            Shader.SetUniform("view", SceneManager.ActiveScene.activeCam.ViewMatrix);
            Shader.SetUniform("projection", SceneManager.ActiveScene.activeCam.ProjectionMatrix);

            Shader.SetUniform("color", Values.lightColor / 255f);
            Shader.SetUniform("ForceLight", Values.ForceLightScene);


            for( int i = 0; i < ball.Count; i++)
            {
                ball[i].Position += ball[i].Direction;

                if(Delete(ball[i].Position.X) || Delete(ball[i].Position.Y) || Delete(ball[i].Position.Z))
                {
                    ball.RemoveAt(i);
                    break;
                }

                Matrix4 model = Matrix4.Identity;
                model = model * Matrix4.CreateScale(0.5f);
                model = model * Matrix4.CreateTranslation(ball[i].Position);

                Shader.SetUniform("model", model);
                MyGame.Sphere.RenderSphere();
            }

        }
        private bool Delete(float num)
        {
            if(num > SceneManager.ActiveScene.activeCam.DistanceOfView || num < -SceneManager.ActiveScene.activeCam.DistanceOfView)
            {
                return true;
            }
            else
            {
                return false;
            }
                
        }

        public void Dispose()
        {
            Shader.Dispose();
        }
    }
}