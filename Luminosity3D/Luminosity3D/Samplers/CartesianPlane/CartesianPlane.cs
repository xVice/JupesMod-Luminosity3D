using Luminosity3D.Builtin;
using Luminosity3D.Rendering;
using Luminosity3DRendering;
using Luminosity3DScening;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Luminosity3D
{
    public class CartesianPlane
    {
        private ShaderProgram shader;
        // private List<Meshe> Lines;
        private VertexArrayObject VaoX;
        private VertexArrayObject VaoY;
        private VertexArrayObject VaoZ;
        private int count;
        public unsafe CartesianPlane()
        {
            shader = new ShaderProgram("./Samplers/CartesianPlane/shader.vert", "./Samplers/CartesianPlane/shader.frag");

            var pointsX = new List<Vector3>();
            var pointsY = new List<Vector3>();
            var pointsZ = new List<Vector3>();

            for(int i = -1000; i < 1010; i += 10)
            {
                pointsX.Add( new Vector3((float)i, 0f, 0f));
                pointsY.Add( new Vector3(0f, (float)i, 0f));
                pointsZ.Add( new Vector3(0f, 0f, (float)i));
            }
            
            count = pointsX.Count;

            VaoX = new VertexArrayObject();
            var VboX = new BufferObject<Vector3>(pointsX.ToArray(), BufferTarget.ArrayBuffer);
            VaoX.LinkBufferObject(ref VboX);
            VaoX.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, sizeof(Vector3), 0);

            VaoY = new VertexArrayObject();
            var VboY = new BufferObject<Vector3>(pointsY.ToArray(), BufferTarget.ArrayBuffer);
            VaoY.LinkBufferObject(ref VboY);
            VaoY.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, sizeof(Vector3), 0);

            VaoZ = new VertexArrayObject();
            var VboZ = new BufferObject<Vector3>(pointsZ.ToArray(), BufferTarget.ArrayBuffer);
            VaoZ.LinkBufferObject(ref VboZ);
            VaoZ.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, sizeof(Vector3), 0);

        }
        public void RenderFrame()
        {
            shader.Use();


            var activecam = SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
            shader.SetUniform("model", Matrix4.Identity);
            shader.SetUniform("view", activecam.ViewMatrix);
            shader.SetUniform("projection", activecam.ProjectionMatrix);
            shader.SetUniform("Light", 15.0f);

            VaoX.Bind();
            shader.SetUniform("colorPositive", Color4.Red);
            shader.SetUniform("colorNegative", Color4.BlueViolet);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, count);

            VaoY.Bind();
            shader.SetUniform("colorPositive", Color4.Blue);
            shader.SetUniform("colorNegative", Color4.SkyBlue);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, count);

            VaoZ.Bind();
            shader.SetUniform("colorPositive", Color4.Green);
            shader.SetUniform("colorNegative", Color4.YellowGreen);

            GL.DrawArrays(PrimitiveType.LineLoop, 0, count);
        }
    }
}