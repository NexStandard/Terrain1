﻿using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;

namespace Terrain;
[DataContract]
public class TerrainGridRenderData
{
    public ModelComponent ModelComponent { get; set; }
    public int Size { get; set; } = 0;
    public int CellSize { get; set; } = 0;
    public Stride.Graphics.Buffer VertexBuffer { get; set; }
    public Stride.Graphics.Buffer IndexBuffer { get; set; }
}
