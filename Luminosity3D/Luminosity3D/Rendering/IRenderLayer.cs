using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3DRendering
{
    public interface IRenderLayer
    {

        Renderer.RenderLayerType LayerType { get; } // Ind
        // Indicates if this layer should handle ImGui rendering
        void Render();
    }

}
