using NexYaml;
using NexYaml.Serialization;
using NexYaml.Serializers;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using System.Collections.Generic;
using Terrain;
using Terrain1.Tools;
using static Stride.Graphics.Buffer;

namespace Terrain1.Drawing;
[Display("Standard Vertex Draw")]
[DataContract]
public class TerrainStandardVertexDraw : TerrainVertexDraw
{
    [DataMemberIgnore]
    public Dictionary<Int2, TerrainVertex> NewData = new();

    [DataMemberIgnore]
    public Dictionary<Int2, int> VertexColorMaterialMapping { get; } = new();

    [Display(Browsable = false)]
    public List<TerrainVertex> VertexCpuBuffer;

    public override void Rebuild()
    {
        VertexCpuBuffer = Grid.GenerateVertices();
        var indices = Grid.GenerateIndices();
        GridRenderData.Size = Grid.Size;
        GridRenderData.CellSize = Grid.CellSize;
        GridRenderData.IndexBuffer = Index.New(TerrainGraphicsDevice, indices, GraphicsResourceUsage.Default);
        GridRenderData.VertexBuffer = Vertex.New(TerrainGraphicsDevice, VertexCpuBuffer.ToArray(), GraphicsResourceUsage.Default);
        
        var mesh = new Mesh
        {
            Draw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleList,
                DrawCount = indices.Length,
                IndexBuffer = new IndexBufferBinding(GridRenderData.IndexBuffer, true, indices.Length),
                VertexBuffers = new[] { new VertexBufferBinding(GridRenderData.VertexBuffer, TerrainVertex.Layout, GridRenderData.VertexBuffer.ElementCount) },
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
        var x = (TerrainVertex)vertex;
        VertexCpuBuffer[index] = x;
        // Update the vertex directly in the GPU buffer at the correct offset
        GridRenderData.VertexBuffer.SetData(
            TerrainGraphicsCommandList,
            ref x, // Update only this single vertex
            index * TerrainVertex.Layout.CalculateSize()
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
    private TerrainVertex SetIndexAsColor(TerrainVertex vertex, int colorIndex)
    {
        var x1 = colorIndex / 4;
        var y = colorIndex % 4;
        return vertex;
    }
    public TerrainVertex[] GenerateVertices(TerrainGrid grid)
    {
        var vertexCount = (grid.Size + 1) * (grid.Size + 1);
        var vertices = new TerrainVertex[vertexCount];

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

                vertices[index] = new TerrainVertex()
                {
                    Position = position,
                    Normal = Vector3.UnitY,
                };
            }
        }

        return vertices;
    }
}