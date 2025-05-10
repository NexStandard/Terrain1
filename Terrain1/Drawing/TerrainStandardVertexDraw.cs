using Microsoft.Win32;
using NexYaml;
using NexYaml.Serialization;
using NexYaml.Serializers;
using Stride.Core;
using Stride.Core.Extensions;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Terrain;
using Terrain1.Tools;
using Terrain1.YamlExtensions;
using static Stride.Graphics.Buffer;

namespace Terrain1.Drawing;
[Display("Standard Vertex Draw")]
[DataContract]
public class TerrainStandardVertexDraw : TerrainVertexDraw
{
    [DataMemberIgnore]
    public Dictionary<Int2, VertexPositionNormalColorTexture> NewData = new();

    [DataMemberIgnore]
    public Dictionary<Int2, int> VertexColorMaterialMapping { get; } = new();

    [Display(Browsable = false)]
    public List<VertexPositionNormalColorTexture> VertexCpuBuffer;

    public override void Rebuild()
    {
        var vertexCount = (Grid.Size + 1) * (Grid.Size + 1);
        VertexCpuBuffer = Grid.GenerateVertices();
        var indices = Grid.GenerateIndices();
        GridRenderData.Size = Grid.Size;
        GridRenderData.CellSize = Grid.CellSize;
        var indexBuffer = Stride.Graphics.Buffer.Index.New(TerrainGraphicsDevice, indices, GraphicsResourceUsage.Default);
        GridRenderData.IndexBuffer = indexBuffer;
        var vertexBuffer = Vertex.New(TerrainGraphicsDevice, VertexCpuBuffer.ToArray(), GraphicsResourceUsage.Default);
        GridRenderData.VertexBuffer = vertexBuffer;
        var mesh = new Mesh
        {
            Draw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleList,
                DrawCount = indices.Length,
                IndexBuffer = new IndexBufferBinding(GridRenderData.IndexBuffer, true, indices.Length),
                VertexBuffers = new[] { new VertexBufferBinding(GridRenderData.VertexBuffer, VertexPositionNormalColorTexture.Layout, GridRenderData.VertexBuffer.ElementCount) },
            },
            MaterialIndex = 0,
        };
        var model = new Model()
        {
            Meshes = [mesh],
        };

        GridRenderData.ModelComponent.Model.Meshes.Clear();
        GridRenderData.ModelComponent.Model.Meshes.Add(mesh);
        var comp = Grid.Entity.Get<ModelComponent>();
        if (comp == null)
        {
            comp = new ModelComponent()
            {
                Model = model
            };
            Grid.Entity.Add(comp);

        }
        else
        {
            comp.Model = model;
        }
        if (Grid.Material != null)
        {
            GridRenderData.ModelComponent.Materials.Clear();
            GridRenderData.ModelComponent.Materials.Add(0, Grid.Material);
            comp.Model.Meshes[0].MaterialIndex = 0;
            comp.Model.Materials.Add(Grid.Material);
        }
    }
    private void Rebuild(List<VertexPositionNormalColorTexture> vertexPositionNormalColorTextures)
    {
        var vertexCount = (Grid.Size + 1) * (Grid.Size + 1);
        VertexCpuBuffer = vertexPositionNormalColorTextures;
        var indices = Grid.GenerateIndices();
        GridRenderData.Size = Grid.Size;
        GridRenderData.CellSize = Grid.CellSize;
        var indexBuffer = Stride.Graphics.Buffer.Index.New(TerrainGraphicsDevice, indices, GraphicsResourceUsage.Default);
        GridRenderData.IndexBuffer = indexBuffer;
        var vertexBuffer = Vertex.New(TerrainGraphicsDevice, VertexCpuBuffer.ToArray(), GraphicsResourceUsage.Default);
        GridRenderData.VertexBuffer = vertexBuffer;
        var mesh = new Mesh
        {
            Draw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleList,
                DrawCount = indices.Length,
                IndexBuffer = new IndexBufferBinding(GridRenderData.IndexBuffer, true, indices.Length),
                VertexBuffers = new[] { new VertexBufferBinding(GridRenderData.VertexBuffer, VertexPositionNormalColorTexture.Layout, GridRenderData.VertexBuffer.ElementCount) },
            },
            MaterialIndex = 0,
        };
        var model = new Model()
        {
            Meshes = [mesh],
        };

        GridRenderData.ModelComponent.Model.Meshes.Clear();
        GridRenderData.ModelComponent.Model.Meshes.Add(mesh);
        var comp = Grid.Entity.Get<ModelComponent>();
        if (comp == null)
        {
            comp = new ModelComponent()
            {
                Model = model
            };
            Grid.Entity.Add(comp);

        }
        else
        {
            comp.Model = model;
        }
        if (Grid.Material != null)
        {
            GridRenderData.ModelComponent.Materials.Clear();
            GridRenderData.ModelComponent.Materials.Add(0, Grid.Material);
            comp.Model.Meshes[0].MaterialIndex = 0;
            comp.Model.Materials.Add(Grid.Material);
        }
    }
    public override void Rebuild(Int2 cell, IVertex vertex)
    {
        if(Grid is null)
        {
            throw new System.Exception();
        }
        var index = (cell.Y * (Grid.Size + 1)) + cell.X;
        var x = (VertexPositionNormalColorTexture)vertex;
        VertexCpuBuffer[index] = x;
        // Update the vertex directly in the GPU buffer at the correct offset
        GridRenderData.VertexBuffer.SetData(
            TerrainGraphicsCommandList,
            ref x, // Update only this single vertex
            index * VertexPositionNormalColorTexture.Layout.CalculateSize()
        );
    }

    public void SetVertexHeight(Int2 cell, float height)
    {
        var index = (cell.Y * (Grid.Size + 1)) + cell.X;
        var currentVertex = VertexCpuBuffer[index];
        currentVertex.Position.Y = currentVertex.Position.Y + height;

        VertexCpuBuffer[index] = currentVertex;
        Rebuild(cell, currentVertex);
    }

    public void SetVertexColor(Int2 cell, int colorLayerIndex)
    {

        var index = (cell.Y * (Grid.Size + 1)) + cell.X;

        var currentVertex = VertexCpuBuffer[index];
        currentVertex = SetIndexAsColor(currentVertex, colorLayerIndex);

        VertexCpuBuffer[index] = currentVertex;
        Rebuild(cell, currentVertex);
    }
    private VertexPositionNormalColorTexture SetIndexAsColor(VertexPositionNormalColorTexture vertex, int colorIndex)
    {
        var x1 = colorIndex / 4;
        var y = colorIndex % 4;
        var color = y switch
        {
            0 => Color.Red,
            1 => Color.Green,
            2 => Color.Blue,
            _ => Color.Black,
        };
        var col = new Color(0, 0, 0, 0);
        if (x1 == 0)
        {
            vertex.Color = color;
            vertex.Color1 = col;
            vertex.Color2 = col;
            vertex.Color3 = col;
            vertex.Color4 = col;
            vertex.Color5 = col;
            vertex.Color6 = col;
        }
        else if(x1 == 1)
        {
            vertex.Color = col;
            vertex.Color1 = color;
            vertex.Color2 = col;
            vertex.Color3 = col;
            vertex.Color4 = col;
            vertex.Color5 = col;
            vertex.Color6 = col;
        }
        else if (x1 == 2)
        {
            vertex.Color = col;
            vertex.Color1 = col;
            vertex.Color2 = color;
            vertex.Color3 = col;
            vertex.Color4 = col;
            vertex.Color5 = col;
            vertex.Color6 = col;
        }
        else if (x1 == 3)
        {
            vertex.Color = col;
            vertex.Color1 = col;
            vertex.Color2 = col;
            vertex.Color3 = color;
            vertex.Color4 = col;
            vertex.Color5 = col;
            vertex.Color6 = col;
        }
        else if (x1 == 4)
        {
            vertex.Color = col;
            vertex.Color1 = col;
            vertex.Color2 = col;
            vertex.Color3 = col;
            vertex.Color4 = color;
            vertex.Color5 = col;
            vertex.Color6 = col;
        }
        else if (x1 == 5)
        {
            vertex.Color = col;
            vertex.Color1 = col;
            vertex.Color2 = col;
            vertex.Color3 = col;
            vertex.Color4 = col;
            vertex.Color5 = color;
            vertex.Color6 = col;
        }
        else if (x1 == 6)
        {
            vertex.Color = col;
            vertex.Color1 = col;
            vertex.Color2 = col;
            vertex.Color3 = col;
            vertex.Color4 = col;
            vertex.Color5 = col;
            vertex.Color6 = color;
        }
        else
        {
            vertex.Color = col;
            vertex.Color1 = col;
            vertex.Color2 = col;
            vertex.Color3 = col;
            vertex.Color4 = col;
            vertex.Color5 = col;
            vertex.Color6 = col;
        }
        return vertex;
    }
    public VertexPositionNormalColorTexture[] GenerateVertices(TerrainGrid grid)
    {
        var vertexCount = (grid.Size + 1) * (grid.Size + 1);
        var vertices = new VertexPositionNormalColorTexture[vertexCount];

        var actualCellWidth = grid.TotalWidth / grid.Size;
        var actualCellHeight = grid.TotalHeight / grid.Size;

        for (var row = 0; row <= grid.Size; row++)
        {
            for (var col = 0; col <= grid.Size; col++)
            {
                var index = (row * (grid.Size + 1)) + col;

                var x = col * actualCellWidth;
                var z = row * actualCellHeight;

                var position = new Vector3(x, grid.GetVertexHeight(col, row), z);

                var color = Color.Black;

                vertices[index] = new VertexPositionNormalColorTexture
                {
                    Position = position,
                    Normal = Vector3.UnitY,
                    TextureCoordinate = new Vector2((float)col / grid.Size, (float)row / grid.Size),
                    Color = color,
                    Color1 = color,
                    Color2 = color,
                    Color3 = color,
                    Color4 = color,
                    Color5 = color,
                    Color6 = color,
                };
            }
        }

        return vertices;
    }
    private IYamlSerializerResolver Create()
    {
        var registry = NexYamlSerializerRegistry.Create(typeof(TerrainStandardVertexDraw).Assembly);
        new ListSerializerFactory().Register(registry);
        var nexYamlSerializerRegistry = new NexYamlSerializerRegistry();
        nexYamlSerializerRegistry.SerializerRegistry = new SerializerRegistry();
        new NexSourceGenerated_Terrain1_ToolsVertexPositionNormalColorTextureHelper().Register(registry);
        new ColorFactory().Register(registry);
        new VectorFactory().Register(registry);
        new Vector2Factory().Register(registry);
        new NexSourceGenerated_Terrain1_DrawingBufferWrapperHelper().Register(registry);
        return registry;
    }
    public override void SaveTransaction()
    {
        var registry = Create();
        var w = new BufferWrapper()
        {
            VertexBuffer = VertexCpuBuffer
        };
        var s = Yaml.Write(w, options: registry);
    }
    public override void LoadTransaction()
    {
        var registry = Create();
    }
}
[DataContract]
public class BufferWrapper
{
    public List<VertexPositionNormalColorTexture> VertexBuffer { get; set; }
}