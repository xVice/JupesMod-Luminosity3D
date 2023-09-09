using Luminosity3DRendering;
using OpenTK.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Rendering
{

    public abstract class GLRenderLayer : IRenderLayer
    {
        public Renderer.RenderLayerType LayerType => Renderer.RenderLayerType.GLRender;
        public Renderer Renderer;

        public GLRenderLayer(Renderer renderer)
        {
            Renderer = renderer;
        }


        public abstract void Render();
    }
}
