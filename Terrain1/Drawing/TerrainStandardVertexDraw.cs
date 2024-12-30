using Stride.CommunityToolkit.Engine;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrain;
using Terrain1.Tools;

namespace Terrain1.Drawing;
[Display("Standard Vertex Draw")]
[DataContract]
public class TerrainStandardVertexDraw : TerrainVertexDraw
{
    public Dictionary<Int2, int> VertexColorMaterialMapping { get; } = new();

    public List<KeyValuePair<Int2, int>> VertexMaterials { get => VertexColorMaterialMapping.ToList(); set => value.ToDictionary((x) => new KeyValuePair<Int2, int>(x.Key, x.Value)); }
    public override void Rebuild(TerrainGrid Grid, TerrainGridRenderData data)
    {
        var vertices = GenerateVertices(Grid);
        var indices = Grid.GenerateIndices();
        data.Size = Grid.Size;
        data.CellSize = Grid.CellSize;
        var indexBuffer = Stride.Graphics.Buffer.Index.New(TerrainGraphicsDevice, indices, GraphicsResourceUsage.Default);
        data.IndexBuffer = indexBuffer;
        var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(TerrainGraphicsDevice, vertices, GraphicsResourceUsage.Default);
        data.VertexBuffer = vertexBuffer;
        var mesh = new Mesh
        {
            Draw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleList,
                DrawCount = indices.Length,
                IndexBuffer = new IndexBufferBinding(data.IndexBuffer, true, indices.Length),
                VertexBuffers = [new VertexBufferBinding(data.VertexBuffer, VertexPositionNormalColorTexture.Layout, data.VertexBuffer.ElementCount)],
            },
            MaterialIndex = 0,
        };
        var model = new Model()
        {
            Meshes = [mesh],
        };
        data.ModelComponent.Model.Meshes.Clear();
        data.ModelComponent.Model.Meshes.Add(mesh);
        Grid.Entity.TryGetComponent<ModelComponent>(out var comp);
        if (comp == null)
        {
            
            comp = new ModelComponent()
            {
                Model = model
            };
            // Grid.Entity.Add(comp);
            
        }
        else
        {
            comp.Model = model;
        }
        if (Grid.Material != null)
        {
            data.ModelComponent.Materials.Clear();
            data.ModelComponent.Materials.Add(0, Grid.Material);
            comp.Model.Meshes[0].MaterialIndex = 0;
            comp.Model.Materials.Add(Grid.Material);
        }
    
        if (Grid.Material != null)
        {
            data.ModelComponent.Materials.Clear();
            data.ModelComponent.Materials.Add(0, Grid.Material);
            comp.Model.Meshes[0].MaterialIndex = 0;
            comp.Model.Materials.Add(Grid.Material);
        }
    }

    public override void Rebuild(TerrainGrid grid, TerrainGridRenderData data, Int2 cell)
    {
        var comp = grid.Entity.Get<ModelComponent>();
        if (grid.Material != null)
        {
            data.ModelComponent.Materials.Clear();
            data.ModelComponent.Materials.Add(0, grid.Material);
            data.ModelComponent.Model.Meshes[0].MaterialIndex = 0;
            data.ModelComponent.Model.Materials.Add(grid.Material);
        }
        
        var x = cell.X * grid.CellSize;
        var z = cell.Y * grid.CellSize;

        var index = (cell.Y * (grid.Size + 1)) + cell.X;

        // Prepare the updated vertex
        var updatedVertex = new VertexPositionNormalColorTexture
        {
            Position = new Vector3(x, grid.GetVertexHeight(cell.X, cell.Y), z),
            Normal = Vector3.UnitY,
            TextureCoordinate = new Vector2(
                (float)cell.X / grid.Size,
                (float)cell.Y / grid.Size
            ),
            Color = Color.Black,
            Color1 = Color.Black
        };
        if (VertexColorMaterialMapping.TryGetValue(new Int2(x, z), out var target))
        {
            var x1 = target / 4;
            var y = target % 4;
            var color = y switch
            {
                0 => Color.Red,
                1 => Color.Green,
                2 => Color.Blue,
                _ => Color.Black,
            };
            if (x1 == 0)
            {
                updatedVertex.Color = color;

            }
            else
            {
                updatedVertex.Color1 = color;
            }
        }
        // Update the vertex directly in the GPU buffer at the correct offset
        data.VertexBuffer.SetData(
            TerrainGraphicsCommandList,
            ref updatedVertex, // Update only this single vertex
            index * VertexPositionNormalColorTexture.Layout.CalculateSize()
        );
    }


    public void SetVertexColor(int x, int y, int colorLayerIndex)
    {
        var k = new Int2(x, y);
        VertexColorMaterialMapping[k] = colorLayerIndex;
        Grid.ModifiedVertices.Add(k);
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
                };
            }
        }

        return vertices;
    }

    public override void PushChanges(TerrainGrid grid, TerrainGridRenderData data)
    {

    }
}
