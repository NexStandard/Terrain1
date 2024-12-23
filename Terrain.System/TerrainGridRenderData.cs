using Stride.Engine;

namespace Terrain;

public class TerrainGridRenderData
{
    public ModelComponent ModelComponent { get; set; }
    public Stride.Graphics.Buffer VertexBuffer { get; set; }
    public Stride.Graphics.Buffer IndexBuffer { get; set; }
}
