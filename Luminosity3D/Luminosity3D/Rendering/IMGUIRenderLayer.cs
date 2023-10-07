
namespace Luminosity3DRendering
{
    public abstract class IMGUIRenderLayer : IRenderLayer
    {
        public Renderer.RenderLayerType LayerType => Renderer.RenderLayerType.ImGui;
        public Renderer Renderer;

        public ImGuiController IMGUI { get => Renderer.IMGUIController; }

        public IMGUIRenderLayer(Renderer renderer)
        {
            Renderer = renderer;
        }


        public abstract void Render();
    }
}
