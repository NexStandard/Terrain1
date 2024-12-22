using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Graphics;
using Stride.Rendering;
using System;

namespace Terrain;
[Display("Terrain", Expand = ExpandRule.Once)]
[DefaultEntityComponentRenderer(typeof(TerrainGridProcessor))]
public class TerrainGrid : StartupScript
{
    [DataMemberRange(1,int.MaxValue)]
    public int SubDivision { get; init; } = 100;
    [DataMemberRange(1, int.MaxValue)]
    public int CellSize { get; init; } = 10;
    public int Columns => SubDivision;
    public int Rows => SubDivision;
    public float CellWidth => CellSize;
    public float CellHeight => CellSize;
    public Material Material { get; set; }

    public VertexPositionNormalTexture[] GenerateVertices()
    {
        var subColumns = Columns ;
        var subRows = Rows ;
        var vertexCount = (subColumns + 1) * (subRows + 1);
        var vertices = new VertexPositionNormalTexture[vertexCount];

        var actualCellWidth = CellWidth / subColumns;
        var actualCellHeight = CellHeight / subRows;

        for (var row = 0; row <= subRows; row++)
        {
            for (var col = 0; col <= subColumns; col++)
            {
                var index = (row * (subColumns + 1)) + col;

                var x = col * actualCellWidth;
                var z = row * actualCellHeight;

                vertices[index] = new VertexPositionNormalTexture
                {
                    Position = new Vector3(x, 0, z),
                    Normal = Vector3.UnitY,
                    TextureCoordinate = new Vector2(
                        (float)col / subColumns,
                        (float)row / subRows
                        )
                };
            }
        }

        return vertices;
    }
    public int[] GenerateIndices()
    {
        var subColumns = Columns;
        var subRows = Rows;

        var quadCount = subColumns * subRows;
        var indices = new int[quadCount * 6];

        var index = 0;
        for (var row = 0; row < subRows; row++)
        {
            for (var col = 0; col < subColumns; col++)
            {
                var topLeft = (row * (subColumns + 1)) + col;
                var topRight = topLeft + 1;
                var bottomLeft = topLeft + subColumns + 1;
                var bottomRight = bottomLeft + 1;

                // Reverse winding order by swapping the order of the indices
                indices[index++] = topLeft;
                indices[index++] = topRight;
                indices[index++] = bottomLeft;

                indices[index++] = bottomLeft;
                indices[index++] = topRight;
                indices[index++] = bottomRight;
            }
        }

        return indices;
    }

}
public class TerrainGridProcessor : EntityProcessor<TerrainGrid, TerrainGridRenderData>, IEntityComponentRenderProcessor
{
    public VisibilityGroup VisibilityGroup { get; set; }

    protected override TerrainGridRenderData GenerateComponentData([NotNull] Entity entity, [NotNull] TerrainGrid component)
    {
        return new TerrainGridRenderData() { ModelComponent = new() { Model = new() } };
    }
    public override void Draw(RenderContext context)
    {
        base.Draw(context);

        var graphicsDevice = Services.GetService<IGraphicsDeviceService>().GraphicsDevice;
        foreach (var grid in ComponentDatas)
        {
            var vertices = grid.Key.GenerateVertices();
            var indices = grid.Key.GenerateIndices();

            var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(graphicsDevice, vertices, GraphicsResourceUsage.Dynamic);
            var indexBuffer = Stride.Graphics.Buffer.Index.New(graphicsDevice, indices, GraphicsResourceUsage.Dynamic);

            var mesh = new Stride.Rendering.Mesh
            {
                Draw = new Stride.Rendering.MeshDraw
                {
                    PrimitiveType = Stride.Graphics.PrimitiveType.TriangleList,
                    DrawCount = indices.Length,
                    IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Length),
                    VertexBuffers = new[] { new VertexBufferBinding(vertexBuffer, VertexPositionNormalTexture.Layout, vertexBuffer.ElementCount) },
                },
                MaterialIndex = 0,
            };
            var model = new Model()
            {
                Meshes = [mesh],

            };

            grid.Value.ModelComponent.Model.Meshes.Clear();
            grid.Value.ModelComponent.Model.Meshes.Add(mesh);

            var comp = grid.Key.Entity.Get<ModelComponent>();
            if (comp == null)
            {
                comp = new ModelComponent()
                {
                    Model = model
                };
                grid.Key.Entity.Add(comp);

            }
            else
            {
                comp.Model = model;
            }
            if (grid.Key.Material != null)
            {
                grid.Value.ModelComponent.Materials.Clear();
                grid.Value.ModelComponent.Materials.Add(0,grid.Key.Material);
                comp.Model.Meshes[0].MaterialIndex = 0;
                comp.Model.Materials.Add(grid.Key.Material);
            }
        }
    }

    protected override void OnEntityComponentRemoved(Entity entity, TerrainGrid component, TerrainGridRenderData data)
    {
        entity.Remove<ModelComponent>();
    }
}
public class TerrainGridRenderData
{
    public ModelComponent ModelComponent { get; set; }
}
