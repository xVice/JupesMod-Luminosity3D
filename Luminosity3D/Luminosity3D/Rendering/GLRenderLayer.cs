using Luminosity3DRendering;

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
