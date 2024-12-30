using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrain;

namespace Terrain1.Drawing;
public abstract class TerrainVertexDraw
{
    [Browsable(false)]
    public TerrainGrid Grid { get; internal set; }
    [Browsable(false)]
    public TerrainGridRenderData GridRenderData { get; internal set; }
    [Browsable(false)]
    public GraphicsDevice TerrainGraphicsDevice { get; set; }
    [Browsable(false)]
    public CommandList TerrainGraphicsCommandList { get; internal set; }
    /// <summary>
    /// Is invoked if the Terrains Render Data has significant changes that require an entire rebuild of the Terrain
    /// </summary>
    /// <param name="grid">The <see cref="TerrainGrid"/> to rebuild from scratch</param>
    /// <param name="data">The <see cref="TerrainGridRenderData"/> which is out of sync of the <paramref name="grid"/></param>
    public abstract void Rebuild(TerrainGrid grid,TerrainGridRenderData data);
    /// <summary>
    /// Is invoked if a single Cell of the <see cref="TerrainGrid"/> is out of synced 
    /// </summary>
    /// <param name="grid">The target <see cref="TerrainGrid"/> to modify the cells Vertex</param>
    /// <param name="data">The render data of the </param>
    /// <param name="cell">The terrain grid cell that requires changes ( not normalized with <see cref="TerrainGrid.CellSize"/> )</param>
    /// <param name="bufferOffsetIndex">The offset location of the Cells Vertex in the <see cref="TerrainGridRenderData.VertexBuffer"/></param>
    public abstract void Rebuild(TerrainGrid grid, TerrainGridRenderData data, Int2 cell);
    public abstract void PushChanges(TerrainGrid grid, TerrainGridRenderData data);
}
