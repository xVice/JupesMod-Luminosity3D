
using Luminosity3D.Builtin;
using Luminosity3D.Rendering;
using Luminosity3DScening;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Luminosity3D
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
            Shader = new ShaderProgram("Samplers/Tinted/shader.vert", "Samplers/Tinted/shader.frag");
        }
        public void RenderFrame()
        {
            Shader.Use();
            var activecam = SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
            Shader.SetUniform("view", activecam.ViewMatrix);
            Shader.SetUniform("projection", activecam.ProjectionMatrix);

            Shader.SetUniform("color", Vector4.One / 255f);
            Shader.SetUniform("ForceLight", 15.0f);


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
                //MyGame.Sphere.RenderSphere();
            }

        }
        private bool Delete(float num)
        {
            if(num > 1500f || num < -1500f)
            {
                return true;
            }
            else
            {
                return false;
            }
                
        }
        public void UpdateFrame()
        {


        }
        public void Dispose()
        {
            Shader.Dispose();
        }
    }
}